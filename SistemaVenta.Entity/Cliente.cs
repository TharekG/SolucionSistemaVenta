using System;
using System.Collections.Generic;

namespace SistemaVenta.Entity;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string? NombreCliente { get; set; }

    public string? RfcCliente { get; set; }

    public string? DireccionFiscal { get; set; }

    public int? IdCodigoPostal { get; set; }

    public string? CorreoElectronico { get; set; }

    public int? IdRegimenFiscal { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public bool? EsActivo { get; set; }

    public virtual CRegimenFiscalSat? IdRegimenFiscalNavigation { get; set; }

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
