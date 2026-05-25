using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.Entity;

namespace SistemaVenta.AplicacionWeb.Utilidades
{
    public class TimbradoService
    {
        private readonly DbventaContext _db;
        private const string WS_URL = "https://ws.urbansa.com/app/timbrado.asmx";
        private const string WS_ACTION = "http://ws.urbansa.com/TimbrarF";   // ← http, NO https
        private const string USUARIO = "FIME";
        private const string PASSWORD = "s9%4ns7q#eGq";
        private const string ID_PREFIX = "111";          // prefijo del grupo

        public TimbradoService(DbventaContext db) => _db = db;

        // ─────────────────────────────────────────────────────────────────
        //  Punto de entrada público
        // ─────────────────────────────────────────────────────────────────
        public async Task<string> TimbrarVenta(int idVenta)
        {
            // 1. Cargar venta con todas sus relaciones
            Venta venta = await _db.Venta
                .Include(v => v.DetalleVenta)
                .Include(v => v.IdClienteNavigation)
                    .ThenInclude(c => c!.IdRegimenFiscalNavigation)
                .Include(v => v.IdUsoCFDINavigation)
                .Include(v => v.IdFormaPagoNavigation)
                .Include(v => v.IdMetodoPagoNavigation)
                .Include(v => v.IdRegimenFiscalNavigation)
                .Include(v => v.IdTipoDeComprobanteNavigation)
                .FirstOrDefaultAsync(v => v.IdVenta == idVenta)
                ?? throw new Exception("Venta no encontrada.");

            // 2. Cargar negocio CON su régimen fiscal
            Negocio negocio = await _db.Negocios
                .Include(n => n.IdRegimenFiscalNavigation)
                .FirstOrDefaultAsync(n => n.IdNegocio == 1)
                ?? throw new Exception("Negocio no configurado.");

            // 3. Verificar que no esté ya timbrada (por UUID en BD o ZIP existente)
            if (!string.IsNullOrEmpty(venta.Uuid))
                throw new Exception($"Esta venta ya fue timbrada. UUID: {venta.Uuid}");

            // Usar mismo formato de nombre que al guardar
            string numVentaGuard = !string.IsNullOrWhiteSpace(venta.NumeroVenta)
                ? venta.NumeroVenta
                : idVenta.ToString().PadLeft(6, '0');
            string zipPath = Path.Combine("wwwroot", "facturas", $"factura_{numVentaGuard}.zip");
            if (File.Exists(zipPath))
                throw new Exception("Esta venta ya fue timbrada anteriormente. Para ver el CFDI usa el botón XML en el historial.");

            // 4. Construir XML y timbrar
            string xml = await BuildXml(venta, negocio);

            byte[] zipBytes = await CallTimbrarF(xml);
            string uuid = ExtractUuidFromZip(zipBytes);

            // 4. Guardar ZIP y también el XML suelto (para visualización)
            string folder = Path.Combine("wwwroot", "facturas");
            // Usar NumeroVenta si existe; si no, formatear igual que el sistema (6 dígitos)
            string numVenta = !string.IsNullOrWhiteSpace(venta.NumeroVenta)
                ? venta.NumeroVenta
                : idVenta.ToString().PadLeft(6, '0');
            Directory.CreateDirectory(folder);
            await File.WriteAllBytesAsync(Path.Combine(folder, $"factura_{numVenta}.zip"), zipBytes);

            // Extraer y guardar el XML por separado
            using (var ms2 = new MemoryStream(zipBytes))
            using (var zip2 = new System.IO.Compression.ZipArchive(ms2, System.IO.Compression.ZipArchiveMode.Read))
            {
                var xmlEntry = zip2.Entries
                    .FirstOrDefault(e => e.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
                if (xmlEntry != null)
                {
                    using var sr = new StreamReader(xmlEntry.Open(), Encoding.UTF8);
                    string xmlTimbrado = sr.ReadToEnd();
                    await File.WriteAllTextAsync(
                        Path.Combine(folder, $"factura_{numVenta}.xml"), xmlTimbrado, Encoding.UTF8);
                }
            }

            return uuid;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Construcción del XML  (estructura idéntica al Form1.cs que funcionó)
        // ─────────────────────────────────────────────────────────────────
        private async Task<string> BuildXml(Venta venta, Negocio negocio)
        {
            // ── Encabezado ────────────────────────────────────────────────
            // idLocal DEBE ser único por timbrado — usamos prefijo+idVenta+sufijo único
            string idLocal = ID_PREFIX + venta.IdVenta.ToString() + Guid.NewGuid().ToString("N")[..6].ToUpper();
            string folio = venta.NumeroVenta ?? venta.IdVenta.ToString();
            string metodoPago = (venta.IdMetodoPagoNavigation?.CMetodoPago ?? "PUE").Trim();
            // Regla SAT CFDI40105: si MetodoPago=PPD entonces FormaPago DEBE ser "99"
            string formaPago = metodoPago == "PPD"
                ? "99"
                : (venta.IdFormaPagoNavigation?.CFormaPago ?? "01").Trim().PadLeft(2, '0');
            string lugarExp = negocio.Codigopostal ?? venta.CodigoPostal ?? "64000";

            // tipoDeComprobante DEBE ser "I", "E", "T", "N" o "P"
            // Si la navegación devuelve null, usamos "I" (Ingreso) por defecto
            // tipoComp DEBE ser exactamente "I", "E", "T", "N" o "P"
            string tipoComp = (venta.IdTipoDeComprobanteNavigation?.CTipoDeComprobante ?? "").Trim();
            if (string.IsNullOrEmpty(tipoComp) || tipoComp.Length > 1)
                tipoComp = "I";   // Ingreso por defecto

            // subTotal y total se recalculan abajo con la fórmula SAT
            // para garantizar: total = subTotal - descuento + impuestos
            decimal subTotal = 0m;   // se recalcula
            decimal total = 0m;   // se recalcula

            // ── Emisor ────────────────────────────────────────────────────
            string rfcEmisor = negocio.Rfc ?? "";
            string nombreEmisor = negocio.Nombre ?? "";
            string regimenEmisor =
                negocio.IdRegimenFiscalNavigation?.CRegimenFiscal
                ?? await GetRegimenCode(negocio.IdRegimenFiscal)
                ?? "601";

            // ── Receptor ──────────────────────────────────────────────────
            string rfcRec, nombreRec, cpRec, regimenRec;

            if (venta.IdClienteNavigation is { } cli)
            {
                rfcRec = cli.RfcCliente ?? "XAXX010101000";
                nombreRec = cli.NombreCliente ?? "Venta Público En General";
                cpRec = cli.IdCodigoPostal?.ToString() ?? lugarExp;
                regimenRec = cli.IdRegimenFiscalNavigation?.CRegimenFiscal
                             ?? await GetRegimenCode(cli.IdRegimenFiscal)
                             ?? "616";
            }
            else
            {
                rfcRec = "XAXX010101000";
                nombreRec = "Venta Público En General";
                cpRec = lugarExp;
                regimenRec = "616";
            }

            string usoCFDI = venta.IdUsoCFDINavigation?.CUsoCFDI ?? "S01";

            // El SAT exige que XAXX/XEXX usen S01 + régimen 616
            bool esPublicoGeneral = rfcRec is "XAXX010101000" or "XEXX010101000";
            if (esPublicoGeneral)
            {
                usoCFDI = "S01";
                regimenRec = "616";
                nombreRec = "Público en General";
            }

            // ── Conceptos ─────────────────────────────────────────────────
            var sb = new StringBuilder();
            decimal descuentoTotal = 0m;
            decimal baseTotal = 0m;
            decimal impuestoTotal = 0m;

            foreach (DetalleVenta dv in venta.DetalleVenta)
            {
                Producto? prod = dv.IdProducto.HasValue
                    ? await _db.Productos
                        .Include(p => p.IdClaveProdServNavigation)
                        .Include(p => p.IdClaveUnidadNavigation)
                        .Include(p => p.IdObjetoImpuestoNavigation)
                        .Include(p => p.IdImpuestoNavigation)
                        .Include(p => p.IdTipoFactorNavigation)
                        .FirstOrDefaultAsync(p => p.IdProducto == dv.IdProducto)
                    : null;

                // SAT claves con fallbacks seguros
                string claveProd = prod?.IdClaveProdServNavigation?.CClaveProdServ ?? "01010101";
                string claveUnidad = prod?.IdClaveUnidadNavigation?.CClaveUnidad ?? "H87";
                string unidad = prod?.IdClaveUnidadNavigation?.Nombre ?? "Pieza";
                string objetoImp = prod?.IdObjetoImpuestoNavigation?.CObjetoImpuesto ?? "02";
                // Limpiar y normalizar códigos SAT (la BD a veces guarda "2" en vez de "002", o espacios)
                string cImpuesto = (prod?.IdImpuestoNavigation?.CImpuesto ?? "002").Trim().PadLeft(3, '0');
                string tipoFactor = (prod?.IdTipoFactorNavigation?.CTipoFactor ?? "Tasa").Trim();
                decimal tasa = prod?.Impuestoproducto ?? 0.16m;
                if (tasa > 1) tasa = tasa / 100m;   // BD guarda 16 → convertir a 0.16

                // Precios en la BD vienen CON IVA incluido → back-calculamos sin IVA
                int cant = dv.Cantidad ?? 1;
                decimal precioConIva = dv.Preciodeventa ?? dv.Precio ?? 0m;
                decimal valUnit = Math.Round(precioConIva / (1 + tasa), 6);
                decimal importe = Math.Round(cant * valUnit, 2);
                decimal desc = 0m;   // descuento de catálogo no aplica al detalle
                decimal baseImp = importe;
                decimal impDin = Math.Round(baseImp * tasa, 2);

                descuentoTotal += desc;
                baseTotal += baseImp;
                impuestoTotal += impDin;

                // Nodo <Traslado> dentro del concepto (sólo si tiene impuesto)
                string traslado = objetoImp == "02"
                    ? $"<Traslado>" +
                      $"<base>{baseImp:F2}</base>" +
                      $"<impuesto>{cImpuesto}</impuesto>" +
                      $"<tipoFactor>{tipoFactor}</tipoFactor>" +
                      $"<tasaOCuota>{tasa:F6}</tasaOCuota>" +
                      $"<importe>{impDin:F2}</importe>" +
                      $"</Traslado>"
                    : "";

                sb.Append(
                    "<Concepto>" +
                    $"<claveProdServ>{claveProd}</claveProdServ>" +
                    "<noIdentificacion></noIdentificacion>" +
                    $"<cantidad>{cant}</cantidad>" +
                    $"<claveUnidad>{claveUnidad}</claveUnidad>" +
                    $"<unidad>{unidad}</unidad>" +
                    $"<descripcion>{Xml(dv.DescripcionProducto ?? "Producto")}</descripcion>" +
                    $"<valorUnitario>{valUnit:F2}</valorUnitario>" +
                    $"<importe>{importe:F2}</importe>" +
                    $"<descuento>{desc:F2}</descuento>" +
                    $"<objetoImp>{objetoImp}</objetoImp>" +
                    traslado +
                    "<rfcACuentaTerceros></rfcACuentaTerceros>" +
                    "<nombreACuentaTerceros></nombreACuentaTerceros>" +
                    "<regimenFiscalACuentaTerceros></regimenFiscalACuentaTerceros>" +
                    "<domicilioFiscalACuentaTerceros></domicilioFiscalACuentaTerceros>" +
                    "<numeroPedimento></numeroPedimento>" +
                    "<cuentaPredial></cuentaPredial>" +
                    "</Concepto>"
                );
            }

            // Nodo global de traslados al final
            string trasladosGlobal = impuestoTotal > 0
                ? "<Traslados>" +
                  $"<base>{baseTotal:F2}</base>" +
                  "<impuesto>002</impuesto>" +
                  "<tipoFactor>Tasa</tipoFactor>" +
                  "<tasaOCuota>0.160000</tasaOCuota>" +
                  $"<importe>{impuestoTotal:F2}</importe>" +
                  "</Traslados>"
                : "";

            // ── Comprobante completo ───────────────────────────────────────
            return
                "<Comprobante>" +
                $"<idLocal>{idLocal}</idLocal>" +
                "<version>4.0</version>" +
                "<serie></serie>" +
                $"<folio>{folio}</folio>" +
                $"<formaPago>{formaPago}</formaPago>" +
                "<condicionesDePago></condicionesDePago>" +
                // subTotal y total calculados desde los conceptos
                $"<subTotal>{baseTotal:F2}</subTotal>" +
                $"<descuento>{descuentoTotal:F2}</descuento>" +
                "<moneda>MXN</moneda>" +
                "<tipoCambio>1.0</tipoCambio>" +
                $"<total>{Math.Round(baseTotal - descuentoTotal + impuestoTotal, 2):F2}</total>" +
                $"<tipoDeComprobante>{tipoComp}</tipoDeComprobante>" +
                "<exportacion>01</exportacion>" +
                $"<metodoPago>{metodoPago}</metodoPago>" +
                $"<lugarExpedicion>{lugarExp}</lugarExpedicion>" +
                "<confirmacion></confirmacion>" +
                "<periodicidad></periodicidad>" +
                "<meses></meses>" +
                "<ano></ano>" +
                "<Relacionado><tipoRelacion></tipoRelacion><uUID></uUID></Relacionado>" +
                $"<regimenFiscal>{regimenEmisor}</regimenFiscal>" +
                "<facAtrAdquirente></facAtrAdquirente>" +
                $"<rfc>{rfcRec}</rfc>" +
                $"<nombre>{Xml(nombreRec)}</nombre>" +
                $"<domicilioFiscalReceptor>{cpRec}</domicilioFiscalReceptor>" +
                "<residenciaFiscal></residenciaFiscal>" +
                "<numRegIdTrib></numRegIdTrib>" +
                $"<regimenFiscalReceptor>{regimenRec}</regimenFiscalReceptor>" +
                $"<usoCFDI>{usoCFDI}</usoCFDI>" +
                sb.ToString() +
                "<totalImpuestosRetenidos></totalImpuestosRetenidos>" +
                $"<totalImpuestosTrasladados>{impuestoTotal:F2}</totalImpuestosTrasladados>" +
                trasladosGlobal +
                "</Comprobante>";
        }

        // ─────────────────────────────────────────────────────────────────
        //  Llamada SOAP
        // ─────────────────────────────────────────────────────────────────
        private async Task<byte[]> CallTimbrarF(string xmlContent)
        {
            string envelope =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" " +
                "xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                "<soap:Body>" +
                "<TimbrarF xmlns=\"http://ws.urbansa.com/\">" +
                $"<Usuario>{USUARIO}</Usuario>" +
                $"<Password>{PASSWORD}</Password>" +
                $"<StrXml>{Xml(xmlContent)}</StrXml>" +
                "</TimbrarF>" +
                "</soap:Body>" +
                "</soap:Envelope>";

            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };

            var body = new StringContent(envelope, Encoding.UTF8, "text/xml");
            body.Headers.Add("SOAPAction", $"\"{WS_ACTION}\"");

            HttpResponseMessage resp = await client.PostAsync(WS_URL, body);
            string raw = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception(
                    $"HTTP {(int)resp.StatusCode}: {raw[..Math.Min(600, raw.Length)]}");

            XDocument doc;
            try { doc = XDocument.Parse(raw); }
            catch { throw new Exception($"Respuesta inválida: {raw[..Math.Min(400, raw.Length)]}"); }

            XNamespace ns = "http://ws.urbansa.com/";
            string? b64 = doc.Descendants(ns + "TimbrarFResult").FirstOrDefault()?.Value;

            if (string.IsNullOrEmpty(b64))
            {
                string? fault = doc.Descendants("faultstring").FirstOrDefault()?.Value;
                throw new Exception(fault ?? "El servicio no devolvió resultado.");
            }

            return Convert.FromBase64String(b64);
        }

        // ─────────────────────────────────────────────────────────────────
        //  Extraer UUID del ZIP
        // ─────────────────────────────────────────────────────────────────
        private static string ExtractUuidFromZip(byte[] zip)
        {
            using var ms = new MemoryStream(zip);
            using var arc = new ZipArchive(ms, ZipArchiveMode.Read);

            ZipArchiveEntry? entry = arc.Entries
                .FirstOrDefault(e => e.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));

            if (entry is null)
                throw new Exception("No hay XML en el ZIP devuelto por Urbansa.");

            using var sr = new StreamReader(entry.Open(), Encoding.UTF8);
            string xml = sr.ReadToEnd();

            XDocument doc = XDocument.Parse(xml);
            XNamespace tfd = "http://www.sat.gob.mx/TimbreFiscalDigital";
            string? uuid = doc.Descendants(tfd + "TimbreFiscalDigital")
                                .FirstOrDefault()?.Attribute("UUID")?.Value;

            return uuid ?? throw new Exception("UUID no encontrado en el CFDI timbrado.");
        }

        // ─────────────────────────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────────────────────────
        private async Task<string?> GetRegimenCode(int? id)
        {
            if (id is null) return null;
            return (await _db.CRegimenFiscalSat.FindAsync(id.Value))?.CRegimenFiscal;
        }

        private static string Xml(string s) =>
            s.Replace("&", "&amp;")
             .Replace("<", "&lt;")
             .Replace(">", "&gt;")
             .Replace("\"", "&quot;")
             .Replace("'", "&apos;");
    }
}
