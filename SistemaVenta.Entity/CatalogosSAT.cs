using System;
using System.Collections.Generic;

namespace SistemaVenta.Entity;

// ─── c_RegimenFiscal_SAT ────────────────────────────────────────────────────
public partial class CRegimenFiscalSat
{
    public int IdRegimenFiscal { get; set; }

    public string? CRegimenFiscal { get; set; }

    public string? Descripcion { get; set; }

    public string? Fisica { get; set; }

    public string? Moral { get; set; }

    public DateTime? FechaInicioVigencia { get; set; }

    public DateTime? FechaFinVigencia { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<Negocio> Negocios { get; set; } = new List<Negocio>();

    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}

// ─── c_Marca ────────────────────────────────────────────────────────────────
public partial class CMarca
{
    public int IdMarca { get; set; }

    public string? CMarcaCode { get; set; }

    public string? Descripcion { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

// ─── c_Medida_Local ─────────────────────────────────────────────────────────
public partial class CMedidaLocal
{
    public int IdMedidaLocal { get; set; }

    public string? CMedidaLocalCode { get; set; }

    public string? Descripcion { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

// ─── c_ClaveUnidad_SAT ──────────────────────────────────────────────────────
public partial class CClaveUnidadSat
{
    public int IdClaveUnidad { get; set; }

    public string? CClaveUnidad { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public string? Nota { get; set; }

    public DateTime? FechaInicioVigencia { get; set; }

    public DateTime? FechaFinVigencia { get; set; }

    public string? Simbolo { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

// ─── c_ClaveProdServ_SAT ────────────────────────────────────────────────────
public partial class CClaveProdServSat
{
    public int IdClaveProdServ { get; set; }
    public string? CClaveProdServ { get; set; }
    public string? Descripcion { get; set; }
    public bool? EsActivo { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

// ─── c_ObjetoImp_SAT ────────────────────────────────────────────────────────
public partial class CObjetoImpSat
{
    public int IdObjetoImpuesto { get; set; }

    public string? CObjetoImpuesto { get; set; }

    public string? Descripcion { get; set; }

    public DateTime? FechaInicioVigencia { get; set; }

    public DateTime? FechaFinVigencia { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

// ─── c_Impuesto_SAT ─────────────────────────────────────────────────────────
public partial class CImpuestoSat
{
    public int IdImpuesto { get; set; }

    public string? CImpuesto { get; set; }

    public string? Descripcion { get; set; }

    public string? Retencion { get; set; }

    public string? Traslado { get; set; }

    public string? LocalOFederal { get; set; }

    public DateTime? FechaInicioVigencia { get; set; }

    public DateTime? FechaFinVigencia { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

// ─── c_TipoFactor_SAT ───────────────────────────────────────────────────────
public partial class CTipoFactorSat
{
    public int IdTipoFactor { get; set; }

    public string? CTipoFactor { get; set; }

    public DateTime? FechaInicioVigencia { get; set; }

    public DateTime? FechaFinVigencia { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

// ─── c_FormaPago_SAT ────────────────────────────────────────────────────────
public partial class CFormaPagoSat
{
    public int IdFormaPago { get; set; }

    public string? CFormaPago { get; set; }

    public string? Descripcion { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}

// ─── c_MetodoPago_SAT ───────────────────────────────────────────────────────
public partial class CMetodoPagoSat
{
    public int IdMetodoPago { get; set; }

    public string? CMetodoPago { get; set; }

    public string? Descripcion { get; set; }

    public DateTime? FechaInicioVigencia { get; set; }

    public DateTime? FechaFinVigencia { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}

// ─── c_UsoCFDI_SAT ──────────────────────────────────────────────────────────
public partial class CUsoCfdiSat
{
    public int IdUsoCFDI { get; set; }

    public string? CUsoCFDI { get; set; }

    public string? Descripcion { get; set; }

    public string? AplicaPersonaFisica { get; set; }

    public string? AplicaPersonaMoral { get; set; }

    public DateTime? FechaInicioVigencia { get; set; }

    public DateTime? FechaFinVigencia { get; set; }

    public string? RegimenFiscalReceptor { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}

// ─── c_TipoDeComprobante_SAT ────────────────────────────────────────────────
public partial class CTipoDeComprobanteSat
{
    public int IdTipoDeComprobante { get; set; }

    public string? CTipoDeComprobante { get; set; }

    public string? Descripcion { get; set; }

    public DateTime? FechaInicioVigencia { get; set; }

    public DateTime? FechaFinVigencia { get; set; }

    public bool? EsActivo { get; set; }

    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}

