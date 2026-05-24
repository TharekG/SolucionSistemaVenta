namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class VMDetalleVenta
    {
        public int? IdProducto { get; set; }
        public string? MarcaProducto { get; set; }
        public string? DescripcionProducto { get; set; }
        public string? CategoriaProducto { get; set; }
        public int? Cantidad { get; set; }
        public decimal? Precio { get; set; }
        public decimal? Total { get; set; }

        // Campos de desglose
        public decimal? Preciodeventa { get; set; }
        public decimal? Subtotalantesdescuento { get; set; }
        public decimal? Descuentoenporcentaje { get; set; }
        public decimal? Descuentoendinero { get; set; }
        public decimal? Subtotalcondescuento { get; set; }
        public decimal? Impuestoenporcentaje { get; set; }
        public decimal? Impuestoendinero { get; set; }
        public decimal? Totalporproducto { get; set; }
    }
}
