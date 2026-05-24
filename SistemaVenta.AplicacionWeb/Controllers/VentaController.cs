using AutoMapper;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.Entity;
using System.Security.Claims;

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

        public VentaController(ITipoDocumentoVentaService tipoDocumentoVentaServicio,
            IVentaService ventaServicio,
            IMapper mapper,
            IConverter converter,
            DbventaContext dbContext)
        {
            _tipoDocumentoVentaServicio = tipoDocumentoVentaServicio;
            _ventaServicio = ventaServicio;
            _mapper = mapper;
            _converter = converter;
            _dbContext = dbContext;
        }

        public IActionResult NuevaVenta()
        {
            return View();
        }

        public IActionResult HistorialVenta()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ListaTipoDocumentoVenta()
        {
            List<VMTipoDocumentoVenta> vmListaTipoDocumentos = _mapper.Map<List<VMTipoDocumentoVenta>>(await _tipoDocumentoVentaServicio.Lista());

            return StatusCode(StatusCodes.Status200OK, vmListaTipoDocumentos);

        }

        [HttpGet]
        public async Task<IActionResult> ObtenerProductos(string busqueda)
        {
            List<VMProducto> vmListaProductos = _mapper.Map<List<VMProducto>>(await _ventaServicio.ObtenerProductos(busqueda));

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
            List<VMVenta> vmHistorialVenta = _mapper.Map<List<VMVenta>> (await _ventaServicio.Historial(numeroVenta, fechaInicio, fechaFin));

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
                    throw new TaskCanceledException("Venta no encontrada.");

                if (!string.IsNullOrEmpty(venta.Uuid))
                    throw new TaskCanceledException("Esta venta ya fue facturada.");

                venta.IdUsoCFDI = modelo.IdUsoCFDI;
                venta.IdRegimenFiscal = modelo.IdRegimenFiscal;
                venta.IdFormaPago = modelo.IdFormaPago;
                venta.IdMetodoPago = modelo.IdMetodoPago;
                venta.IdTipoDeComprobante = modelo.IdTipoDeComprobante;
                venta.CodigoPostal = modelo.CodigoPostal;
                // UUID simulado — en producción vendría del PAC timbrador
                venta.Uuid = Guid.NewGuid().ToString();
                venta.FechaTimbrado = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                gResponse.Estado = true;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }



        public IActionResult MostrarPDFVenta(string numeroVenta)
        {
            string urlPlantillaVista = $"{this.Request.Scheme}://{this.Request.Host}/Plantilla/PDFVenta?numeroVenta={numeroVenta}";

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
                        Page = urlPlantillaVista,
                        WebSettings = new WebSettings()
                        {
                            LoadImages = true,
                            EnableJavascript = true
                        },
                        LoadSettings = new LoadSettings()
                        {
                            BlockLocalFileAccess = false,
                            StopSlowScript = false
                        }
                    }
                }
            };

            var archivoPDF = _converter.Convert(pdf);

            return File(archivoPDF, "application/pdf");

        }


    }
}