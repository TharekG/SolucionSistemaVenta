namespace SistemaVenta.AplicacionWeb.Models.ViewModels
{
    public class VMCliente
    {
        public int IdCliente { get; set; }

        public string? NombreCliente { get; set; }

        public string? RfcCliente { get; set; }

        public string? DireccionFiscal { get; set; }

        public int? IdCodigoPostal { get; set; }

        public string? CorreoElectronico { get; set; }

        public int? IdRegimenFiscal { get; set; }

        // Para mostrar en tabla (viene del navigation)
        public string? DescripcionRegimenFiscal { get; set; }

        public int? EsActivo { get; set; }
    }
}
