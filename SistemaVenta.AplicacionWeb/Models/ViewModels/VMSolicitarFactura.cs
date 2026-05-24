namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class VMSolicitarFactura
    {
        public int IdVenta { get; set; }
        public int? IdUsoCFDI { get; set; }
        public int? IdRegimenFiscal { get; set; }
        public int? IdFormaPago { get; set; }
        public int? IdMetodoPago { get; set; }
        public int? IdTipoDeComprobante { get; set; }
        public string? CodigoPostal { get; set; }
    }
}
