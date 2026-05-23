using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.Entity;

namespace SistemaVenta.DAL.DBContext;

public partial class DbventaContext : DbContext
{
    public DbventaContext() { }

    public DbventaContext(DbContextOptions<DbventaContext> options) : base(options) { }

    // ── Tablas originales ──────────────────────────────────────────────────
    public virtual DbSet<Categoria> Categoria { get; set; }
    public virtual DbSet<Configuracion> Configuracions { get; set; }
    public virtual DbSet<DetalleVenta> DetalleVenta { get; set; }
    public virtual DbSet<Menu> Menus { get; set; }
    public virtual DbSet<Negocio> Negocios { get; set; }
    public virtual DbSet<NumeroCorrelativo> NumeroCorrelativos { get; set; }
    public virtual DbSet<Producto> Productos { get; set; }
    public virtual DbSet<Rol> Rols { get; set; }
    public virtual DbSet<RolMenu> RolMenus { get; set; }
    public virtual DbSet<TipoDocumentoVenta> TipoDocumentoVenta { get; set; }
    public virtual DbSet<Usuario> Usuarios { get; set; }
    public virtual DbSet<Venta> Venta { get; set; }

    // ── Nuevas tablas ──────────────────────────────────────────────────────
    public virtual DbSet<Cliente> Clientes { get; set; }

    // ── Catálogos SAT ──────────────────────────────────────────────────────
    public virtual DbSet<CRegimenFiscalSat> CRegimenFiscalSat { get; set; }
    public virtual DbSet<CMarca> CMarca { get; set; }
    public virtual DbSet<CMedidaLocal> CMedidaLocal { get; set; }
    public virtual DbSet<CClaveUnidadSat> CClaveUnidadSat { get; set; }
    public virtual DbSet<CClaveProdServSat> CClaveProdServSat { get; set; }
    public virtual DbSet<CObjetoImpSat> CObjetoImpSat { get; set; }
    public virtual DbSet<CImpuestoSat> CImpuestoSat { get; set; }
    public virtual DbSet<CTipoFactorSat> CTipoFactorSat { get; set; }
    public virtual DbSet<CFormaPagoSat> CFormaPagoSat { get; set; }
    public virtual DbSet<CMetodoPagoSat> CMetodoPagoSat { get; set; }
    public virtual DbSet<CUsoCfdiSat> CUsoCfdiSat { get; set; }
    public virtual DbSet<CTipoDeComprobanteSat> CTipoDeComprobanteSat { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Categoria ──────────────────────────────────────────────────────
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__Categori__8A3D240C0612049F");
            entity.Property(e => e.IdCategoria).HasColumnName("idCategoria");
            entity.Property(e => e.Descripcion).HasMaxLength(50).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())").HasColumnType("datetime").HasColumnName("fechaRegistro");
        });

        // ── Configuracion ──────────────────────────────────────────────────
        modelBuilder.Entity<Configuracion>(entity =>
        {
            entity.HasNoKey().ToTable("Configuracion");
            entity.Property(e => e.Propiedad).HasMaxLength(50).IsUnicode(false).HasColumnName("propiedad");
            entity.Property(e => e.Recurso).HasMaxLength(50).IsUnicode(false).HasColumnName("recurso");
            entity.Property(e => e.Valor).HasMaxLength(60).IsUnicode(false).HasColumnName("valor");
        });

        // ── Cliente ────────────────────────────────────────────────────────
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PK_Cliente");
            entity.ToTable("Cliente");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.NombreCliente).HasMaxLength(80).IsUnicode(false).HasColumnName("nombreCliente");
            entity.Property(e => e.RfcCliente).HasMaxLength(13).IsUnicode(false).HasColumnName("rfcCliente");
            entity.Property(e => e.DireccionFiscal).HasMaxLength(80).IsUnicode(false).HasColumnName("direccionFiscal");
            entity.Property(e => e.IdCodigoPostal).HasColumnName("idCodigoPostal");
            entity.Property(e => e.CorreoElectronico).HasMaxLength(80).IsUnicode(false).HasColumnName("correoElectronico");
            entity.Property(e => e.IdRegimenFiscal).HasColumnName("idRegimenFiscal");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime").HasColumnName("fechaRegistro");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");

            entity.HasOne(d => d.IdRegimenFiscalNavigation)
                .WithMany(p => p.Clientes)
                .HasForeignKey(d => d.IdRegimenFiscal);
        });

        // ── DetalleVenta ───────────────────────────────────────────────────
        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasKey(e => e.IdDetalleVenta).HasName("PK__DetalleV__BFE2843F88065522");
            entity.Property(e => e.IdDetalleVenta).HasColumnName("idDetalleVenta");
            entity.Property(e => e.IdVenta).HasColumnName("idVenta");
            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            entity.Property(e => e.MarcaProducto).HasMaxLength(100).IsUnicode(false).HasColumnName("marcaProducto");
            entity.Property(e => e.DescripcionProducto).HasMaxLength(100).IsUnicode(false).HasColumnName("descripcionProducto");
            entity.Property(e => e.CategoriaProducto).HasMaxLength(100).IsUnicode(false).HasColumnName("categoriaProducto");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)").HasColumnName("precio");
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)").HasColumnName("total");
            entity.Property(e => e.Preciodeventa).HasColumnType("money").HasColumnName("preciodeventa");
            entity.Property(e => e.Subtotalantesdescuento).HasColumnType("money").HasColumnName("subtotalantesdescuento");
            entity.Property(e => e.Descuentoenporcentaje).HasColumnType("decimal(9, 6)").HasColumnName("descuentoenporcentaje");
            entity.Property(e => e.Descuentoendinero).HasColumnType("money").HasColumnName("descuentoendinero");
            entity.Property(e => e.Subtotalcondescuento).HasColumnType("money").HasColumnName("subtotalcondescuento");
            entity.Property(e => e.Impuestoenporcentaje).HasColumnType("decimal(9, 6)").HasColumnName("impuestoenporcentaje");
            entity.Property(e => e.Impuestoendinero).HasColumnType("money").HasColumnName("impuestoendinero");
            entity.Property(e => e.Totalporproducto).HasColumnType("money").HasColumnName("totalporproducto");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.DetalleVenta)
                .HasForeignKey(d => d.IdVenta).HasConstraintName("FK__DetalleVe__idVen__6E01572D");
        });

        // ── Menu ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.IdMenu).HasName("PK__Menu__C26AF483C75C5383");
            entity.ToTable("Menu");
            entity.Property(e => e.IdMenu).HasColumnName("idMenu");
            entity.Property(e => e.Controlador).HasMaxLength(30).IsUnicode(false).HasColumnName("controlador");
            entity.Property(e => e.Descripcion).HasMaxLength(30).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())").HasColumnType("datetime").HasColumnName("fechaRegistro");
            entity.Property(e => e.Icono).HasMaxLength(30).IsUnicode(false).HasColumnName("icono");
            entity.Property(e => e.IdMenuPadre).HasColumnName("idMenuPadre");
            entity.Property(e => e.PaginaAccion).HasMaxLength(30).IsUnicode(false).HasColumnName("paginaAccion");
            entity.HasOne(d => d.IdMenuPadreNavigation).WithMany(p => p.InverseIdMenuPadreNavigation)
                .HasForeignKey(d => d.IdMenuPadre).HasConstraintName("FK__Menu__idMenuPadr__4AB81AF0");
        });

        // ── Negocio ────────────────────────────────────────────────────────
        modelBuilder.Entity<Negocio>(entity =>
        {
            entity.HasKey(e => e.IdNegocio).HasName("PK__Negocio__70E1E107CCE3EE6E");
            entity.ToTable("Negocio");
            entity.Property(e => e.IdNegocio).ValueGeneratedNever().HasColumnName("idNegocio");
            entity.Property(e => e.UrlLogo).HasMaxLength(500).IsUnicode(false).HasColumnName("urlLogo");
            entity.Property(e => e.NombreLogo).HasMaxLength(100).IsUnicode(false).HasColumnName("nombreLogo");
            entity.Property(e => e.NumeroDocumento).HasMaxLength(50).IsUnicode(false).HasColumnName("numeroDocumento");
            entity.Property(e => e.Nombre).HasMaxLength(50).IsUnicode(false).HasColumnName("nombre");
            entity.Property(e => e.Correo).HasMaxLength(50).IsUnicode(false).HasColumnName("correo");
            entity.Property(e => e.Direccion).HasMaxLength(50).IsUnicode(false).HasColumnName("direccion");
            entity.Property(e => e.Telefono).HasMaxLength(50).IsUnicode(false).HasColumnName("telefono");
            entity.Property(e => e.PorcentajeImpuesto).HasColumnType("decimal(10, 2)").HasColumnName("porcentajeImpuesto");
            entity.Property(e => e.SimboloMoneda).HasMaxLength(5).IsUnicode(false).HasColumnName("simboloMoneda");
            entity.Property(e => e.Rfc).HasMaxLength(13).IsUnicode(false).HasColumnName("rfc");
            entity.Property(e => e.Codigopostal).HasMaxLength(5).IsUnicode(false).HasColumnName("codigopostal");
            entity.Property(e => e.IdRegimenFiscal).HasColumnName("idRegimenFiscal");

            entity.HasOne(d => d.IdRegimenFiscalNavigation)
                .WithMany(p => p.Negocios)
                .HasForeignKey(d => d.IdRegimenFiscal);
        });

        // ── NumeroCorrelativo ──────────────────────────────────────────────
        modelBuilder.Entity<NumeroCorrelativo>(entity =>
        {
            entity.HasKey(e => e.IdNumeroCorrelativo).HasName("PK__NumeroCo__25FB547EAC9C61DC");
            entity.ToTable("NumeroCorrelativo");
            entity.Property(e => e.IdNumeroCorrelativo).HasColumnName("idNumeroCorrelativo");
            entity.Property(e => e.CantidadDigitos).HasColumnName("cantidadDigitos");
            entity.Property(e => e.FechaActualizacion).HasColumnType("datetime").HasColumnName("fechaActualizacion");
            entity.Property(e => e.Gestion).HasMaxLength(100).IsUnicode(false).HasColumnName("gestion");
            entity.Property(e => e.UltimoNumero).HasColumnName("ultimoNumero");
        });

        // ── Producto ───────────────────────────────────────────────────────
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PK__Producto__07F4A1323E9A7E5C");
            entity.ToTable("Producto");
            entity.Property(e => e.IdProducto).HasColumnName("idProducto");
            entity.Property(e => e.CodigoBarra).HasMaxLength(50).IsUnicode(false).HasColumnName("codigoBarra");
            entity.Property(e => e.IdMarca).HasColumnName("idMarca");
            entity.Property(e => e.Marca).HasMaxLength(50).IsUnicode(false).HasColumnName("marca");
            entity.Property(e => e.Modelo).HasMaxLength(50).IsUnicode(false).HasColumnName("modelo");
            entity.Property(e => e.Descripcion).HasMaxLength(100).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.IdCategoria).HasColumnName("idCategoria");
            entity.Property(e => e.Stock).HasColumnName("stock");
            entity.Property(e => e.UrlImagen).HasMaxLength(500).IsUnicode(false).HasColumnName("urlImagen");
            entity.Property(e => e.NombreImagen).HasMaxLength(100).IsUnicode(false).HasColumnName("nombreImagen");
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)").HasColumnName("precio");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())").HasColumnType("datetime").HasColumnName("fechaRegistro");
            entity.Property(e => e.Preciocompra).HasColumnType("money").HasColumnName("preciocompra");
            entity.Property(e => e.Precioventa).HasColumnType("money").HasColumnName("precioventa");
            entity.Property(e => e.Descuento).HasColumnType("decimal(9, 6)").HasColumnName("descuento");
            entity.Property(e => e.IdMedidaLocal).HasColumnName("idMedidaLocal");
            entity.Property(e => e.IdClaveUnidad).HasColumnName("idClaveUnidad");
            entity.Property(e => e.IdClaveProdServ).HasColumnName("idClaveProdServ");
            entity.Property(e => e.IdObjetoImpuesto).HasColumnName("idObjetoImpuesto");
            entity.Property(e => e.IdImpuesto).HasColumnName("idImpuesto");
            entity.Property(e => e.IdTipoFactor).HasColumnName("idTipoFactor");
            entity.Property(e => e.Impuestoproducto).HasColumnType("decimal(9, 6)").HasColumnName("impuestoproducto");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCategoria).HasConstraintName("FK_Producto_Categoria");
            entity.HasOne(d => d.IdMarcaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdMarca);
            entity.HasOne(d => d.IdMedidaLocalNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdMedidaLocal);
            entity.HasOne(d => d.IdClaveUnidadNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdClaveUnidad);
            entity.HasOne(d => d.IdClaveProdServNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdClaveProdServ);
            entity.HasOne(d => d.IdObjetoImpuestoNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdObjetoImpuesto);
            entity.HasOne(d => d.IdImpuestoNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdImpuesto);
            entity.HasOne(d => d.IdTipoFactorNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdTipoFactor);
        });

        // ── Rol ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Rol__3C872F7601DF27BB");
            entity.ToTable("Rol");
            entity.Property(e => e.IdRol).HasColumnName("idRol");
            entity.Property(e => e.Descripcion).HasMaxLength(30).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())").HasColumnType("datetime").HasColumnName("fechaRegistro");
        });

        // ── RolMenu ────────────────────────────────────────────────────────
        modelBuilder.Entity<RolMenu>(entity =>
        {
            entity.HasKey(e => e.IdRolMenu).HasName("PK__RolMenu__CD2045D85C6BC3CF");
            entity.ToTable("RolMenu");
            entity.Property(e => e.IdRolMenu).HasColumnName("idRolMenu");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())").HasColumnType("datetime").HasColumnName("fechaRegistro");
            entity.Property(e => e.IdMenu).HasColumnName("idMenu");
            entity.Property(e => e.IdRol).HasColumnName("idRol");
            entity.HasOne(d => d.IdMenuNavigation).WithMany(p => p.RolMenus)
                .HasForeignKey(d => d.IdMenu).HasConstraintName("FK__RolMenu__idMenu__52593CB8");
            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.RolMenus)
                .HasForeignKey(d => d.IdRol).HasConstraintName("FK__RolMenu__idRol__5165187F");
        });

        // ── TipoDocumentoVenta ─────────────────────────────────────────────
        modelBuilder.Entity<TipoDocumentoVenta>(entity =>
        {
            entity.HasKey(e => e.IdTipoDocumentoVenta).HasName("PK__TipoDocu__A9D59AEEFAF8CB8B");
            entity.Property(e => e.IdTipoDocumentoVenta).HasColumnName("idTipoDocumentoVenta");
            entity.Property(e => e.Descripcion).HasMaxLength(50).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())").HasColumnType("datetime").HasColumnName("fechaRegistro");
        });

        // ── Usuario ────────────────────────────────────────────────────────
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__645723A66E944060");
            entity.ToTable("Usuario");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Nombre).HasMaxLength(50).IsUnicode(false).HasColumnName("nombre");
            entity.Property(e => e.Correo).HasMaxLength(50).IsUnicode(false).HasColumnName("correo");
            entity.Property(e => e.Telefono).HasMaxLength(15).IsUnicode(false).HasColumnName("telefono");
            entity.Property(e => e.IdRol).HasColumnName("idRol");
            entity.Property(e => e.UrlFoto).HasMaxLength(500).IsUnicode(false).HasColumnName("urlFoto");
            entity.Property(e => e.NombreFoto).HasMaxLength(100).IsUnicode(false).HasColumnName("nombreFoto");
            entity.Property(e => e.Clave).HasMaxLength(100).IsUnicode(false).HasColumnName("clave");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())").HasColumnType("datetime").HasColumnName("fechaRegistro");
            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol).HasConstraintName("FK__Usuario__idRol__59FA5E80");
        });

        // ── Venta ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.IdVenta).HasName("PK__Venta__077D5614C4FEC6FD");
            entity.Property(e => e.IdVenta).HasColumnName("idVenta");
            entity.Property(e => e.NumeroVenta).HasMaxLength(6).IsUnicode(false).HasColumnName("numeroVenta");
            entity.Property(e => e.IdTipoDocumentoVenta).HasColumnName("idTipoDocumentoVenta");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.DocumentoCliente).HasMaxLength(10).IsUnicode(false).HasColumnName("documentoCliente");
            entity.Property(e => e.NombreCliente).HasMaxLength(20).IsUnicode(false).HasColumnName("nombreCliente");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(10, 2)").HasColumnName("subTotal");
            entity.Property(e => e.ImpuestoTotal).HasColumnType("decimal(10, 2)").HasColumnName("impuestoTotal");
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)").HasColumnName("Total");
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())").HasColumnType("datetime").HasColumnName("fechaRegistro");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.SubTotalVenta).HasColumnType("money").HasColumnName("subTotalVenta");
            entity.Property(e => e.DescuentoVenta).HasColumnType("money").HasColumnName("descuentoVenta");
            entity.Property(e => e.ImpuestoVenta).HasColumnType("money").HasColumnName("impuestoVenta");
            entity.Property(e => e.TotalVenta).HasColumnType("money").HasColumnName("totalVenta");
            entity.Property(e => e.IdUsoCFDI).HasColumnName("idUsoCFDI");
            entity.Property(e => e.IdRegimenFiscal).HasColumnName("idRegimenFiscal");
            entity.Property(e => e.IdFormaPago).HasColumnName("idFormaPago");
            entity.Property(e => e.IdMetodoPago).HasColumnName("idMetodoPago");
            entity.Property(e => e.CodigoPostal).HasMaxLength(5).IsUnicode(false).HasColumnName("CodigoPostal");
            entity.Property(e => e.IdTipoDeComprobante).HasColumnName("idTipoDeComprobante");
            entity.Property(e => e.Uuid).HasMaxLength(36).IsUnicode(false).HasColumnName("uuid");
            entity.Property(e => e.FechaTimbrado).HasColumnType("datetime").HasColumnName("fechaTimbrado");

            entity.HasOne(d => d.IdTipoDocumentoVentaNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdTipoDocumentoVenta).HasConstraintName("FK__Venta__idTipoDoc__6477ECF3");
            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdUsuario).HasConstraintName("FK__Venta__idUsuario__656C112C");
            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.IdCliente).HasConstraintName("FK_Venta_Cliente");
            entity.HasOne(d => d.IdUsoCFDINavigation).WithMany(p => p.Ventas)
                .HasForeignKey(d => d.IdUsoCFDI);
            entity.HasOne(d => d.IdRegimenFiscalNavigation).WithMany(p => p.Ventas)
                .HasForeignKey(d => d.IdRegimenFiscal);
            entity.HasOne(d => d.IdFormaPagoNavigation).WithMany(p => p.Ventas)
                .HasForeignKey(d => d.IdFormaPago);
            entity.HasOne(d => d.IdMetodoPagoNavigation).WithMany(p => p.Ventas)
                .HasForeignKey(d => d.IdMetodoPago);
            entity.HasOne(d => d.IdTipoDeComprobanteNavigation).WithMany(p => p.Ventas)
                .HasForeignKey(d => d.IdTipoDeComprobante);
        });

        // ── Catálogos SAT ──────────────────────────────────────────────────
        modelBuilder.Entity<CRegimenFiscalSat>(entity =>
        {
            entity.HasKey(e => e.IdRegimenFiscal).HasName("PK_c_RegimenFiscal_SAT");
            entity.ToTable("c_RegimenFiscal_SAT");
            entity.Property(e => e.IdRegimenFiscal).HasColumnName("idRegimenFiscal");
            entity.Property(e => e.CRegimenFiscal).HasMaxLength(3).IsUnicode(false).HasColumnName("c_RegimenFiscal");
            entity.Property(e => e.Descripcion).HasMaxLength(90).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.Fisica).HasMaxLength(2).IsUnicode(false).HasColumnName("fisica");
            entity.Property(e => e.Moral).HasMaxLength(2).IsUnicode(false).HasColumnName("moral");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
        });

        modelBuilder.Entity<CMarca>(entity =>
        {
            entity.HasKey(e => e.IdMarca).HasName("PK_Marca");
            entity.ToTable("c_Marca");
            entity.Property(e => e.IdMarca).HasColumnName("idMarca");
            entity.Property(e => e.CMarcaCode).HasMaxLength(15).IsUnicode(false).HasColumnName("c_Marca");
            entity.Property(e => e.Descripcion).HasMaxLength(50).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
        });

        modelBuilder.Entity<CMedidaLocal>(entity =>
        {
            entity.HasKey(e => e.IdMedidaLocal).HasName("PK_Medida_Local");
            entity.ToTable("c_Medida_Local");
            entity.Property(e => e.IdMedidaLocal).HasColumnName("idMedidaLocal");
            entity.Property(e => e.CMedidaLocalCode).HasMaxLength(10).IsUnicode(false).HasColumnName("c_MedidaLocal");
            entity.Property(e => e.Descripcion).HasMaxLength(50).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
        });

        modelBuilder.Entity<CClaveUnidadSat>(entity =>
        {
            entity.HasKey(e => e.IdClaveUnidad).HasName("PK_c_ClaveUnidad");
            entity.ToTable("c_ClaveUnidad_SAT");
            entity.Property(e => e.IdClaveUnidad).HasColumnName("idClaveUnidad");
            entity.Property(e => e.CClaveUnidad).HasMaxLength(3).IsUnicode(false).HasColumnName("c_ClaveUnidad");
            entity.Property(e => e.Nombre).HasMaxLength(120).IsUnicode(false).HasColumnName("nombre");
            entity.Property(e => e.Descripcion).HasMaxLength(560).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.Simbolo).HasMaxLength(30).IsUnicode(false).HasColumnName("simbolo");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
        });

        modelBuilder.Entity<CClaveProdServSat>(entity =>
        {
            entity.HasKey(e => e.IdClaveProdServ).HasName("PK_c_ClaveProdServ_SAT");
            entity.ToTable("c_ClaveProdServ_SAT");
            entity.Property(e => e.IdClaveProdServ).HasColumnName("idClaveProdServ");
            entity.Property(e => e.CClaveProdServ).HasMaxLength(8).IsUnicode(false).HasColumnName("c_ClaveProdServ");
            entity.Property(e => e.Descripcion).HasMaxLength(160).IsUnicode(false).HasColumnName("Descripcion");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
        });

        modelBuilder.Entity<CObjetoImpSat>(entity =>
        {
            entity.HasKey(e => e.IdObjetoImpuesto).HasName("PK_c_ObjetoImp_1");
            entity.ToTable("c_ObjetoImp_SAT");
            entity.Property(e => e.IdObjetoImpuesto).HasColumnName("idObjetoImpuesto");
            entity.Property(e => e.CObjetoImpuesto).HasMaxLength(2).IsUnicode(false).HasColumnName("c_ObjetoImpuesto");
            entity.Property(e => e.Descripcion).HasMaxLength(50).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
        });

        modelBuilder.Entity<CImpuestoSat>(entity =>
        {
            entity.HasKey(e => e.IdImpuesto).HasName("PK_c_Impuesto_SAT");
            entity.ToTable("c_Impuesto_SAT");
            entity.Property(e => e.IdImpuesto).HasColumnName("idImpuesto");
            entity.Property(e => e.CImpuesto).HasMaxLength(2).IsUnicode(false).HasColumnName("c_Impuesto");
            entity.Property(e => e.Descripcion).HasMaxLength(10).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.Retencion).HasMaxLength(2).IsUnicode(false).HasColumnName("retencion");
            entity.Property(e => e.Traslado).HasMaxLength(2).IsUnicode(false).HasColumnName("traslado");
            entity.Property(e => e.LocalOFederal).HasMaxLength(7).IsUnicode(false).HasColumnName("local_O_Federal");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
        });

        modelBuilder.Entity<CTipoFactorSat>(entity =>
        {
            entity.HasKey(e => e.IdTipoFactor).HasName("PK_c_TipoFactor_SAT");
            entity.ToTable("c_TipoFactor_SAT");
            entity.Property(e => e.IdTipoFactor).HasColumnName("idTipoFactor");
            entity.Property(e => e.CTipoFactor).HasMaxLength(6).IsUnicode(false).HasColumnName("c_TipoFactor");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
        });

        modelBuilder.Entity<CFormaPagoSat>(entity =>
        {
            entity.HasKey(e => e.IdFormaPago).HasName("PK_c_FormaPago_SAT");
            entity.ToTable("c_FormaPago_SAT");
            entity.Property(e => e.IdFormaPago).HasColumnName("idFormaPago");
            entity.Property(e => e.CFormaPago).HasMaxLength(2).IsUnicode(false).HasColumnName("c_FormaPago");
            entity.Property(e => e.Descripcion).HasMaxLength(40).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
        });

        modelBuilder.Entity<CMetodoPagoSat>(entity =>
        {
            entity.HasKey(e => e.IdMetodoPago).HasName("PK_c_MetodoPago_SAT");
            entity.ToTable("c_MetodoPago_SAT");
            entity.Property(e => e.IdMetodoPago).HasColumnName("idMetodoPago");
            entity.Property(e => e.CMetodoPago).HasMaxLength(3).IsUnicode(false).HasColumnName("c_MetodoPago");
            entity.Property(e => e.Descripcion).HasMaxLength(50).IsUnicode(false).HasColumnName("Descripcion");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
        });

        modelBuilder.Entity<CUsoCfdiSat>(entity =>
        {
            entity.HasKey(e => e.IdUsoCFDI).HasName("PK_c_UsoCFDI_SAT");
            entity.ToTable("c_UsoCFDI_SAT");
            entity.Property(e => e.IdUsoCFDI).HasColumnName("idUsoCFDI");
            entity.Property(e => e.CUsoCFDI).HasMaxLength(4).IsUnicode(false).HasColumnName("c_UsoCFDI");
            entity.Property(e => e.Descripcion).HasMaxLength(90).IsUnicode(false).HasColumnName("Descripcion");
            entity.Property(e => e.AplicaPersonaFisica).HasMaxLength(2).IsUnicode(false).HasColumnName("Aplica_persona_fisica");
            entity.Property(e => e.AplicaPersonaMoral).HasMaxLength(2).IsUnicode(false).HasColumnName("Aplica_persona_moral");
            entity.Property(e => e.RegimenFiscalReceptor).HasMaxLength(100).IsUnicode(false).HasColumnName("Regimen_fiscal_receptor");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
        });

        modelBuilder.Entity<CTipoDeComprobanteSat>(entity =>
        {
            entity.HasKey(e => e.IdTipoDeComprobante).HasName("PK_c_TipoDeComprobante_SAT");
            entity.ToTable("c_TipoDeComprobante_SAT");
            entity.Property(e => e.IdTipoDeComprobante).HasColumnName("idTipoDeComprobante");
            entity.Property(e => e.CTipoDeComprobante).HasMaxLength(1).IsUnicode(false).HasColumnName("c_TipoDeComprobante");
            entity.Property(e => e.Descripcion).HasMaxLength(10).IsUnicode(false).HasColumnName("descripcion");
            entity.Property(e => e.EsActivo).HasColumnName("esActivo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

