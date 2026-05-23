using System;
using System.Collections.Generic;
using System.Text;

using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Interfaces
{
    public interface IMenuService
    {

        Task<List<Menu>> ObtenerMenus(int idUsuario);

    }
}
