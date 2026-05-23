using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Interfaces
{
    public interface IProductoService
    {

        Task<List<Producto>> Lista();

        Task<Producto> Crear(Producto entidad, Stream imagen = null, string NombreImagen = "");

        Task<Producto> Editar(Producto entidad, Stream imagen = null, string NombreImagen = "");

        Task<bool> Eliminar(int id);

    }
}
