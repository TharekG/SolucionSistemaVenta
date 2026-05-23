using System;
using System.Collections.Generic;

namespace SistemaVenta.Entity;

public partial class Producto
{
    public int IdProducto { get; set; }

    public string? CodigoBarra { get; set; }

    public int? IdMarca { get; set; }

    public string? Marca { get; set; }

    public string? Modelo { get; set; }

    public string? Descripcion { get; set; }

    public int? IdCategoria { get; set; }

    public int? Stock { get; set; }

    public string? UrlImagen { get; set; }

    public string? NombreImagen { get; set; }

    public decimal? Precio { get; set; }

    public bool? EsActivo { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public decimal? Preciocompra { get; set; }

    public decimal? Precioventa { get; set; }

    public decimal? Descuento { get; set; }

    public int? IdMedidaLocal { get; set; }

    public int? IdClaveUnidad { get; set; }

    public int? IdClaveProdServ { get; set; }

    public int? IdObjetoImpuesto { get; set; }

    public int? IdImpuesto { get; set; }

    public int? IdTipoFactor { get; set; }

    public decimal? Impuestoproducto { get; set; }

    public virtual Categoria? IdCategoriaNavigation { get; set; }

    public virtual CMarca? IdMarcaNavigation { get; set; }

    public virtual CMedidaLocal? IdMedidaLocalNavigation { get; set; }

    public virtual CClaveUnidadSat? IdClaveUnidadNavigation { get; set; }

    public virtual CClaveProdServSat? IdClaveProdServNavigation { get; set; }

    public virtual CObjetoImpSat? IdObjetoImpuestoNavigation { get; set; }

    public virtual CImpuestoSat? IdImpuestoNavigation { get; set; }

    public virtual CTipoFactorSat? IdTipoFactorNavigation { get; set; }
}
