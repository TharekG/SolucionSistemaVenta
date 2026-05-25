using AutoMapper;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.Entity;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    [Authorize]
    public class VentaController : Controller
    {
        private readonly ITipoDocumentoVentaService _tipoDocumentoVentaServicio;
        private readonly IVentaService _ventaServicio;
        private readonly IMapper _mapper;
        private readonly IConverter _converter;
        private readonly DbventaContext _dbContext;
        private readonly TimbradoService _timbradoService;

        public VentaController(ITipoDocumentoVentaService tipoDocumentoVentaServicio,
            IVentaService ventaServicio,
            IMapper mapper,
            IConverter converter,
            DbventaContext dbContext,
            TimbradoService timbradoService)
        {
            _tipoDocumentoVentaServicio = tipoDocumentoVentaServicio;
            _ventaServicio = ventaServicio;
            _mapper = mapper;
            _converter = converter;
            _dbContext = dbContext;
            _timbradoService = timbradoService;
        }

        public IActionResult NuevaVenta() => View();
        public IActionResult HistorialVenta() => View();

        [HttpGet]
        public async Task<IActionResult> ListaTipoDocumentoVenta()
        {
            List<VMTipoDocumentoVenta> vmListaTipoDocumentos =
                _mapper.Map<List<VMTipoDocumentoVenta>>(await _tipoDocumentoVentaServicio.Lista());
            return StatusCode(StatusCodes.Status200OK, vmListaTipoDocumentos);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerProductos(string busqueda)
        {
            List<VMProducto> vmListaProductos =
                _mapper.Map<List<VMProducto>>(await _ventaServicio.ObtenerProductos(busqueda));
            return StatusCode(StatusCodes.Status200OK, vmListaProductos);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarVenta([FromBody] VMVenta modelo)
        {
            GenericResponse<VMVenta> gResponse = new GenericResponse<VMVenta>();
            try
            {
                ClaimsPrincipal claimsUser = HttpContext.User;
                string idUsuario = claimsUser.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .Select(c => c.Value).SingleOrDefault();
                modelo.IdUsuario = int.Parse(idUsuario);
                Venta venta_creada = await _ventaServicio.Registrar(_mapper.Map<Venta>(modelo));
                modelo = _mapper.Map<VMVenta>(venta_creada);
                gResponse.Estado = true;
                gResponse.Objeto = modelo;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.InnerException?.Message ?? ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpGet]
        public async Task<IActionResult> Historial(string numeroVenta, string fechaInicio, string fechaFin)
        {
            List<VMVenta> vmHistorialVenta =
                _mapper.Map<List<VMVenta>>(await _ventaServicio.Historial(numeroVenta, fechaInicio, fechaFin));
            return StatusCode(StatusCodes.Status200OK, vmHistorialVenta);
        }

        [HttpGet]
        public async Task<IActionResult> ListaUsoCFDI()
        {
            var lista = await _dbContext.CUsoCfdiSat
                .Where(r => r.EsActivo == true)
                .Select(r => new { r.IdUsoCFDI, r.CUsoCFDI, r.Descripcion })
                .ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        [HttpGet]
        public async Task<IActionResult> ListaRegimenFiscal()
        {
            var lista = await _dbContext.CRegimenFiscalSat
                .Where(r => r.EsActivo == true)
                .Select(r => new { r.IdRegimenFiscal, r.CRegimenFiscal, r.Descripcion })
                .ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        [HttpGet]
        public async Task<IActionResult> ListaFormaPago()
        {
            var lista = await _dbContext.CFormaPagoSat
                .Where(r => r.EsActivo == true)
                .Select(r => new { r.IdFormaPago, r.CFormaPago, r.Descripcion })
                .ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        [HttpGet]
        public async Task<IActionResult> ListaMetodoPago()
        {
            var lista = await _dbContext.CMetodoPagoSat
                .Where(r => r.EsActivo == true)
                .Select(r => new { r.IdMetodoPago, r.CMetodoPago, r.Descripcion })
                .ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        [HttpGet]
        public async Task<IActionResult> ListaTipoComprobante()
        {
            var lista = await _dbContext.CTipoDeComprobanteSat
                .Where(r => r.EsActivo == true)
                .Select(r => new { r.IdTipoDeComprobante, r.CTipoDeComprobante, r.Descripcion })
                .ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        // ── Solicitar Factura ─────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> SolicitarFactura([FromBody] VMSolicitarFactura modelo)
        {
            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                Venta venta = await _dbContext.Venta
                    .FirstOrDefaultAsync(v => v.IdVenta == modelo.IdVenta);

                if (venta == null)
                    throw new Exception("Venta no encontrada.");

                if (!string.IsNullOrEmpty(venta.Uuid))
                    throw new Exception("Esta venta ya fue timbrada anteriormente. Para ver el CFDI usa el botón XML en el historial.");

                // Guardar campos CFDI en la venta
                venta.IdUsoCFDI = modelo.IdUsoCFDI;
                venta.IdRegimenFiscal = modelo.IdRegimenFiscal;
                venta.IdFormaPago = modelo.IdFormaPago;
                venta.IdMetodoPago = modelo.IdMetodoPago;
                venta.IdTipoDeComprobante = modelo.IdTipoDeComprobante;
                venta.CodigoPostal = modelo.CodigoPostal;
                await _dbContext.SaveChangesAsync();

                // Timbrar
                string uuid = await _timbradoService.TimbrarVenta(modelo.IdVenta);

                venta.Uuid = uuid;
                venta.FechaTimbrado = DateTime.Now;
                await _dbContext.SaveChangesAsync();

                gResponse.Estado = true;
                gResponse.Mensaje = uuid;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = LimpiarMensajeError(ex.Message);
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        // ── Ver XML del CFDI ──────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> VerXmlFactura(int idVenta)
        {
            // Buscar por NumeroVenta (el nombre real del archivo)
            var venta = await _dbContext.Venta.FindAsync(idVenta);
            string numVenta = venta?.NumeroVenta ?? idVenta.ToString();

            string path = Path.Combine("wwwroot", "facturas", $"factura_{numVenta}.xml");
            if (!System.IO.File.Exists(path))
                return NotFound("El XML de esta factura no está disponible.");

            string xml = await System.IO.File.ReadAllTextAsync(path, System.Text.Encoding.UTF8);
            return Content(xml, "text/plain", System.Text.Encoding.UTF8);
        }

        // ── PDF ───────────────────────────────────────────────────────────────────
        public IActionResult MostrarPDFVenta(string numeroVenta)
        {
            string urlPlantillaVista =
                $"{this.Request.Scheme}://{this.Request.Host}/Plantilla/PDFVenta?numeroVenta={numeroVenta}";

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings()
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                },
                Objects =
                {
                    new ObjectSettings()
                    {
                        Page         = urlPlantillaVista,
                        WebSettings  = new WebSettings()  { LoadImages = true, EnableJavascript = true },
                        LoadSettings = new LoadSettings() { BlockLocalFileAccess = false, StopSlowScript = false }
                    }
                }
            };

            var archivoPDF = _converter.Convert(pdf);
            return File(archivoPDF, "application/pdf");
        }

        // ── Helper: limpiar mensajes de error SOAP ────────────────────────────────
        private static string LimpiarMensajeError(string error)
        {
            try
            {
                // Si no tiene XML SOAP, es un mensaje ya limpio → devolver directo
                if (!error.Contains("<soap:") && !error.Contains("<?xml"))
                    return error;

                // 1. <ErrorMessage> de Urbansa (el más específico)
                var m1 = Regex.Match(error, @"<ErrorMessage>\s*([^<]+?)\s*</ErrorMessage>",
                                     RegexOptions.Singleline);
                if (m1.Success) return m1.Groups[1].Value.Trim();

                // 2. Exception (N): MENSAJE dentro de faultstring
                var m2 = Regex.Match(error, @"Exception \(\d+\):\s*([^\n<]+)",
                                     RegexOptions.Singleline);
                if (m2.Success) return m2.Groups[1].Value.Trim();

                // 3. Texto dentro de <faultstring> directo
                var m3 = Regex.Match(error, @"<faultstring>[^:]+:\s*([^\n<]+)",
                                     RegexOptions.Singleline);
                if (m3.Success) return m3.Groups[1].Value.Trim();

                // 4. Fallback
                return "Error al timbrar la factura. Verifique los datos e intente de nuevo.";
            }
            catch
            {
                return "Error al procesar la factura.";
            }
        }
    }
}
