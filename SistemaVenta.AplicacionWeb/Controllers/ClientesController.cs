using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IClienteService _clienteServicio;

        public ClientesController(IMapper mapper, IClienteService clienteServicio)
        {
            _mapper = mapper;
            _clienteServicio = clienteServicio;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<VMCliente> vmClienteLista = _mapper.Map<List<VMCliente>>(await _clienteServicio.Lista());
            return StatusCode(StatusCodes.Status200OK, new { data = vmClienteLista });
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] VMCliente modelo)
        {
            GenericResponse<VMCliente> gResponse = new GenericResponse<VMCliente>();
            try
            {
                Cliente cliente_creado = await _clienteServicio.Crear(_mapper.Map<Cliente>(modelo));
                modelo = _mapper.Map<VMCliente>(cliente_creado);
                gResponse.Estado = true;
                gResponse.Objeto = modelo;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] VMCliente modelo)
        {
            GenericResponse<VMCliente> gResponse = new GenericResponse<VMCliente>();
            try
            {
                Cliente cliente_editado = await _clienteServicio.Editar(_mapper.Map<Cliente>(modelo));
                modelo = _mapper.Map<VMCliente>(cliente_editado);
                gResponse.Estado = true;
                gResponse.Objeto = modelo;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int IdCliente)
        {
            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                gResponse.Estado = await _clienteServicio.Eliminar(IdCliente);
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPorRfc(string rfc)
        {
            GenericResponse<VMCliente> gResponse = new GenericResponse<VMCliente>();
            try
            {
                Cliente cliente = await _clienteServicio.ObtenerPorRfc(rfc);
                gResponse.Estado = cliente != null;
                gResponse.Objeto = cliente != null ? _mapper.Map<VMCliente>(cliente) : null;
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

