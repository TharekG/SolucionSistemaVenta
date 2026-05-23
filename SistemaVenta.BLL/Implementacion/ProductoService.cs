using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class ProductoService : IProductoService
    {
        private readonly IGenericRepository<Producto> _repositorio;
        // Ruta local donde se guardan las imágenes (relativa a wwwroot)
        private readonly string _carpetaImagenes = Path.Combine("wwwroot", "imagenes", "producto");

        public ProductoService(IGenericRepository<Producto> repositorio)
        {
            _repositorio = repositorio;
            // Crear carpeta si no existe
            if (!Directory.Exists(_carpetaImagenes))
                Directory.CreateDirectory(_carpetaImagenes);
        }

        public async Task<List<Producto>> Lista()
        {
            IQueryable<Producto> query = await _repositorio.Consultar();
            return query
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.IdMarcaNavigation)
                .Include(p => p.IdMedidaLocalNavigation)
                .Include(p => p.IdClaveUnidadNavigation)
                .Include(p => p.IdClaveProdServNavigation)
                .Include(p => p.IdObjetoImpuestoNavigation)
                .Include(p => p.IdImpuestoNavigation)
                .Include(p => p.IdTipoFactorNavigation)
                .ToList();
        }

        public async Task<Producto> Crear(Producto entidad, Stream? imagen = null, string NombreImagen = "")
        {
            Producto? producto_existe = await _repositorio.Obtener(p => p.CodigoBarra == entidad.CodigoBarra);
            if (producto_existe != null)
                throw new TaskCanceledException("El código de barra ya existe");

            try
            {
                entidad.NombreImagen = NombreImagen;

                if (imagen != null)
                {
                    string rutaArchivo = Path.Combine(_carpetaImagenes, NombreImagen);
                    using (var fs = new FileStream(rutaArchivo, FileMode.Create))
                        await imagen.CopyToAsync(fs);

                    entidad.UrlImagen = $"/imagenes/producto/{NombreImagen}";
                }

                Producto producto_creado = await _repositorio.Crear(entidad);

                if (producto_creado.IdProducto == 0)
                    throw new TaskCanceledException("No se pudo crear el producto.");

                IQueryable<Producto> query = await _repositorio.Consultar(p => p.IdProducto == producto_creado.IdProducto);

                producto_creado = query
                    .Include(p => p.IdCategoriaNavigation)
                    .Include(p => p.IdMarcaNavigation)
                    .Include(p => p.IdMedidaLocalNavigation)
                    .Include(p => p.IdClaveUnidadNavigation)
                    .Include(p => p.IdClaveProdServNavigation)
                    .Include(p => p.IdObjetoImpuestoNavigation)
                    .Include(p => p.IdImpuestoNavigation)
                    .Include(p => p.IdTipoFactorNavigation)
                    .First();

                return producto_creado;
            }
            catch { throw; }
        }

        public async Task<Producto> Editar(Producto entidad, Stream? imagen = null, string NombreImagen = "")
        {
            Producto? producto_existe = await _repositorio.Obtener(
                p => p.CodigoBarra == entidad.CodigoBarra && p.IdProducto != entidad.IdProducto);

            if (producto_existe != null)
                throw new TaskCanceledException("El código de barra ya existe.");

            try
            {
                IQueryable<Producto> queryProducto = await _repositorio.Consultar(p => p.IdProducto == entidad.IdProducto);
                Producto producto_para_editar = queryProducto.First();

                // ── Campos originales ──────────────────────────────────────
                producto_para_editar.CodigoBarra = entidad.CodigoBarra;
                producto_para_editar.Descripcion = entidad.Descripcion;
                producto_para_editar.IdCategoria = entidad.IdCategoria;
                producto_para_editar.Stock = entidad.Stock;
                producto_para_editar.Precio = entidad.Precio;
                producto_para_editar.EsActivo = entidad.EsActivo;

                // ── Campos nuevos ──────────────────────────────────────────
                producto_para_editar.IdMarca = entidad.IdMarca;
                producto_para_editar.Marca = entidad.Marca;           // campo texto legacy
                producto_para_editar.Modelo = entidad.Modelo;
                producto_para_editar.Preciocompra = entidad.Preciocompra;
                producto_para_editar.Precioventa = entidad.Precioventa;
                producto_para_editar.Descuento = entidad.Descuento;
                producto_para_editar.IdMedidaLocal = entidad.IdMedidaLocal;
                producto_para_editar.IdClaveUnidad = entidad.IdClaveUnidad;
                producto_para_editar.IdClaveProdServ = entidad.IdClaveProdServ;
                producto_para_editar.IdObjetoImpuesto = entidad.IdObjetoImpuesto;
                producto_para_editar.IdImpuesto = entidad.IdImpuesto;
                producto_para_editar.IdTipoFactor = entidad.IdTipoFactor;
                producto_para_editar.Impuestoproducto = entidad.Impuestoproducto;

                // ── Imagen ─────────────────────────────────────────────────
                if (imagen != null)
                {
                    // Eliminar imagen anterior si existe
                    if (!string.IsNullOrEmpty(producto_para_editar.NombreImagen))
                    {
                        string rutaAnterior = Path.Combine(_carpetaImagenes, producto_para_editar.NombreImagen);
                        if (File.Exists(rutaAnterior)) File.Delete(rutaAnterior);
                    }

                    string rutaNueva = Path.Combine(_carpetaImagenes, NombreImagen);
                    using (var fs = new FileStream(rutaNueva, FileMode.Create))
                        await imagen.CopyToAsync(fs);

                    producto_para_editar.NombreImagen = NombreImagen;
                    producto_para_editar.UrlImagen = $"/imagenes/producto/{NombreImagen}";
                }

                bool respuesta = await _repositorio.Editar(producto_para_editar);
                if (!respuesta)
                    throw new TaskCanceledException("No se pudo editar el producto.");

                Producto producto_editado = queryProducto
                    .Include(p => p.IdCategoriaNavigation)
                    .Include(p => p.IdMarcaNavigation)
                    .Include(p => p.IdMedidaLocalNavigation)
                    .Include(p => p.IdClaveUnidadNavigation)
                    .Include(p => p.IdClaveProdServNavigation)
                    .Include(p => p.IdObjetoImpuestoNavigation)
                    .Include(p => p.IdImpuestoNavigation)
                    .Include(p => p.IdTipoFactorNavigation)
                    .First();

                return producto_editado;
            }
            catch { throw; }
        }

        public async Task<bool> Eliminar(int idProducto)
        {
            try
            {
                Producto? producto_encontrado = await _repositorio.Obtener(p => p.IdProducto == idProducto);
                if (producto_encontrado == null)
                    throw new TaskCanceledException("El producto no existe");

                string nombreImagen = producto_encontrado.NombreImagen ?? "";

                bool respuesta = await _repositorio.Eliminar(producto_encontrado);

                if (respuesta && !string.IsNullOrEmpty(nombreImagen))
                {
                    string ruta = Path.Combine(_carpetaImagenes, nombreImagen);
                    if (File.Exists(ruta)) File.Delete(ruta);
                }

                return true;
            }
            catch { throw; }
        }
    }
}
