namespace SistemaVenta.AplicacionWeb.Models.ViewModels;

public class VMProducto
{
    public int IdProducto { get; set; }
    public string? CodigoBarra { get; set; }

    // Marca
    public int? IdMarca { get; set; }
    public string? DescripcionMarca { get; set; }

    // Modelo (texto libre)
    public string? Modelo { get; set; }
    public string? Descripcion { get; set; }

    public int? IdCategoria { get; set; }
    public string? DescripcionCategoria { get; set; }

    public int? Stock { get; set; }
    public string? UrlImagen { get; set; }
    public string? NombreImagen { get; set; }

    // Precios
    public decimal? Precio { get; set; }          // precio original
    public decimal? Preciocompra { get; set; }
    public decimal? Precioventa { get; set; }

    // Descuento: se guarda como decimal(9,6) ej: 0.150000 = 15%
    public decimal? Descuento { get; set; }

    public bool? EsActivo { get; set; }

    // ── Catálogos SAT ────────────────────────────────────────────────────
    public int? IdMedidaLocal { get; set; }
    public string? DescripcionMedidaLocal { get; set; }

    public int? IdClaveUnidad { get; set; }
    public string? DescripcionClaveUnidad { get; set; }

    public int? IdClaveProdServ { get; set; }
    public string? DescripcionClaveProdServ { get; set; }

    public int? IdObjetoImpuesto { get; set; }
    public string? DescripcionObjetoImpuesto { get; set; }

    public int? IdImpuesto { get; set; }
    public string? DescripcionImpuesto { get; set; }

    public int? IdTipoFactor { get; set; }
    public string? DescripcionTipoFactor { get; set; }

    // Porcentaje de impuesto calculado: ej 0.160000 = 16%
    public decimal? Impuestoproducto { get; set; }
}
