using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.Entity;
using Microsoft.EntityFrameworkCore;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    [Authorize]
    public class ProductoController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProductoService _productoService;
        private readonly DbventaContext _dbContext;

        public ProductoController(IMapper mapper, IProductoService productoService, DbventaContext dbContext)
        {
            _mapper = mapper;
            _productoService = productoService;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        // ── Catálogos SAT ────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ListaMarca()
        {
            var lista = await _dbContext.CMarca
                .Where(r => r.EsActivo == true)
                .Select(r => new { r.IdMarca, r.CMarcaCode, r.Descripcion })
                .ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        [HttpGet]
        public async Task<IActionResult> ListaMedidaLocal()
        {
            var lista = await _dbContext.CMedidaLocal
                .Where(r => r.EsActivo == true)
                .Select(r => new { r.IdMedidaLocal, r.CMedidaLocalCode, r.Descripcion })
                .ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        [HttpGet]
        public async Task<IActionResult> ListaClaveUnidad()
        {
            var lista = await _dbContext.CClaveUnidadSat
                .Where(r => r.EsActivo == true)
                .Select(r => new { r.IdClaveUnidad, r.CClaveUnidad, r.Nombre })
                .ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        [HttpGet]
        public async Task<IActionResult> ListaClaveProdServ()
        {
            var lista = await _dbContext.CClaveProdServSat
                .Where(r => r.EsActivo == true)
                .Select(r => new { r.IdClaveProdServ, r.CClaveProdServ, r.Descripcion })
                .ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        [HttpGet]
        public async Task<IActionResult> ListaObjetoImpuesto()
        {
            var lista = await _dbContext.CObjetoImpSat
                .Where(r => r.EsActivo == true)
                .Select(r => new { r.IdObjetoImpuesto, r.CObjetoImpuesto, r.Descripcion })
                .ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        [HttpGet]
        public async Task<IActionResult> ListaImpuesto()
        {
            var lista = await _dbContext.CImpuestoSat
                .Where(r => r.EsActivo == true)
                .Select(r => new { r.IdImpuesto, r.CImpuesto, r.Descripcion })
                .ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        [HttpGet]
        public async Task<IActionResult> ListaTipoFactor()
        {
            var lista = await _dbContext.CTipoFactorSat
                .Where(r => r.EsActivo == true)
                .Select(r => new { r.IdTipoFactor, r.CTipoFactor })
                .ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        // ── CRUD ─────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<VMProducto> vmLista = _mapper.Map<List<VMProducto>>(await _productoService.Lista());
            return StatusCode(StatusCodes.Status200OK, new { data = vmLista });
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromForm] IFormFile imagen, [FromForm] string modelo)
        {
            GenericResponse<VMProducto> gResponse = new GenericResponse<VMProducto>();
            try
            {
                VMProducto vmProducto = JsonConvert.DeserializeObject<VMProducto>(modelo)!;
                string nombreImagen = "";
                Stream? imagenStream = null;

                if (imagen != null)
                {
                    string nombre_en_codigo = Guid.NewGuid().ToString("N");
                    string extension = Path.GetExtension(imagen.FileName);
                    nombreImagen = string.Concat(nombre_en_codigo, extension);
                    imagenStream = imagen.OpenReadStream();
                }

                Producto producto_creado = await _productoService.Crear(
                    _mapper.Map<Producto>(vmProducto), imagenStream, nombreImagen);
                vmProducto = _mapper.Map<VMProducto>(producto_creado);

                gResponse.Estado = true;
                gResponse.Objeto = vmProducto;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromForm] IFormFile imagen, [FromForm] string modelo)
        {
            GenericResponse<VMProducto> gResponse = new GenericResponse<VMProducto>();
            try
            {
                VMProducto vmProducto = JsonConvert.DeserializeObject<VMProducto>(modelo)!;
                string nombreImagen = vmProducto.NombreImagen ?? "";
                Stream? imagenStream = null;

                if (imagen != null)
                {
                    string nombre_en_codigo = Guid.NewGuid().ToString("N");
                    string extension = Path.GetExtension(imagen.FileName);
                    nombreImagen = string.Concat(nombre_en_codigo, extension);
                    imagenStream = imagen.OpenReadStream();
                }

                Producto producto_editado = await _productoService.Editar(
                    _mapper.Map<Producto>(vmProducto), imagenStream, nombreImagen);
                vmProducto = _mapper.Map<VMProducto>(producto_editado);

                gResponse.Estado = true;
                gResponse.Objeto = vmProducto;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int idProducto)
        {
            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                bool tieneVentas = await _dbContext.DetalleVenta
                    .AnyAsync(d => d.IdProducto == idProducto);

                if (tieneVentas)
                {
                    gResponse.Estado = false;
                    gResponse.Mensaje = "No se puede eliminar el producto porque tiene ventas registradas.";
                    return StatusCode(StatusCodes.Status200OK, gResponse);
                }

                gResponse.Estado = await _productoService.Eliminar(idProducto);
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }
    }
}
