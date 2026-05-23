using System;
using System.Collections.Generic;

namespace SistemaVenta.Entity;

public partial class Venta
{
    public int IdVenta { get; set; }

    public string? NumeroVenta { get; set; }

    public int? IdTipoDocumentoVenta { get; set; }

    public int? IdUsuario { get; set; }

    public string? DocumentoCliente { get; set; }

    public string? NombreCliente { get; set; }

    public decimal? SubTotal { get; set; }

    public decimal? ImpuestoTotal { get; set; }

    public decimal? Total { get; set; }

    public DateTime? FechaRegistro { get; set; }

    // Cliente vinculado
    public int? IdCliente { get; set; }

    // Campos de desglose de venta
    public decimal? SubTotalVenta { get; set; }

    public decimal? DescuentoVenta { get; set; }

    public decimal? ImpuestoVenta { get; set; }

    public decimal? TotalVenta { get; set; }

    // Campos CFDI / Factura
    public int? IdUsoCFDI { get; set; }

    public int? IdRegimenFiscal { get; set; }

    public int? IdFormaPago { get; set; }

    public int? IdMetodoPago { get; set; }

    public string? CodigoPostal { get; set; }

    public int? IdTipoDeComprobante { get; set; }

    public string? Uuid { get; set; }

    public DateTime? FechaTimbrado { get; set; }

    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();

    public virtual TipoDocumentoVenta? IdTipoDocumentoVentaNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual Cliente? IdClienteNavigation { get; set; }

    public virtual CUsoCfdiSat? IdUsoCFDINavigation { get; set; }

    public virtual CRegimenFiscalSat? IdRegimenFiscalNavigation { get; set; }

    public virtual CFormaPagoSat? IdFormaPagoNavigation { get; set; }

    public virtual CMetodoPagoSat? IdMetodoPagoNavigation { get; set; }

    public virtual CTipoDeComprobanteSat? IdTipoDeComprobanteNavigation { get; set; }
}
