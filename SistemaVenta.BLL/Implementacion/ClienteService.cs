using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class ClienteService : IClienteService
    {
        private readonly IGenericRepository<Cliente> _repositorio;

        public ClienteService(IGenericRepository<Cliente> repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<Cliente>> Lista()
        {
            IQueryable<Cliente> query = await _repositorio.Consultar();
            return query.Include(c => c.IdRegimenFiscalNavigation).ToList();
        }

        public async Task<Cliente> Crear(Cliente entidad)
        {
            try
            {
                Cliente existe = await _repositorio.Obtener(c => c.RfcCliente == entidad.RfcCliente);
                if (existe != null)
                    throw new TaskCanceledException("Ya existe un cliente con ese RFC.");

                Cliente creado = await _repositorio.Crear(entidad);
                if (creado.IdCliente == 0)
                    throw new TaskCanceledException("No se pudo crear el cliente.");

                IQueryable<Cliente> query = await _repositorio.Consultar(c => c.IdCliente == creado.IdCliente);
                return query.Include(c => c.IdRegimenFiscalNavigation).First();
            }
            catch { throw; }
        }

        public async Task<Cliente> Editar(Cliente entidad)
        {
            try
            {
                Cliente existe = await _repositorio.Obtener(
                    c => c.RfcCliente == entidad.RfcCliente && c.IdCliente != entidad.IdCliente);
                if (existe != null)
                    throw new TaskCanceledException("Ya existe otro cliente con ese RFC.");

                Cliente encontrado = await _repositorio.Obtener(c => c.IdCliente == entidad.IdCliente);
                if (encontrado == null)
                    throw new TaskCanceledException("El cliente no existe.");

                encontrado.NombreCliente = entidad.NombreCliente;
                encontrado.RfcCliente = entidad.RfcCliente;
                encontrado.DireccionFiscal = entidad.DireccionFiscal;
                encontrado.IdCodigoPostal = entidad.IdCodigoPostal;
                encontrado.CorreoElectronico = entidad.CorreoElectronico;
                encontrado.IdRegimenFiscal = entidad.IdRegimenFiscal;
                encontrado.EsActivo = entidad.EsActivo;

                bool respuesta = await _repositorio.Editar(encontrado);
                if (!respuesta)
                    throw new TaskCanceledException("No se pudo modificar el cliente.");

                IQueryable<Cliente> query = await _repositorio.Consultar(c => c.IdCliente == encontrado.IdCliente);
                return query.Include(c => c.IdRegimenFiscalNavigation).First();
            }
            catch { throw; }
        }

        public async Task<bool> Eliminar(int idCliente)
        {
            try
            {
                Cliente encontrado = await _repositorio.Obtener(c => c.IdCliente == idCliente);
                if (encontrado == null)
                    throw new TaskCanceledException("El cliente no existe.");

                bool respuesta = await _repositorio.Eliminar(encontrado);
                if (!respuesta)
                    throw new TaskCanceledException("No se pudo eliminar el cliente.");

                return respuesta;
            }
            catch { throw; }
        }

        public async Task<Cliente> ObtenerPorRfc(string rfc)
        {
            try
            {
                IQueryable<Cliente> query = await _repositorio.Consultar(
                    c => c.RfcCliente == rfc && c.EsActivo == true);
                return query.Include(c => c.IdRegimenFiscalNavigation).FirstOrDefault();
            }
            catch { throw; }
        }
    }
}