using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Interfaces
{
    public interface IClienteService
    {
        Task<List<Cliente>> Lista();
        Task<Cliente> Crear(Cliente entidad);
        Task<Cliente> Editar(Cliente entidad);
        Task<bool> Eliminar(int idCliente);
        Task<Cliente> ObtenerPorRfc(string rfc);
    }
}
