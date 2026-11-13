using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Models.Domain;

namespace Nextag.Api.Data;

public partial class NextagContext : DbContext
{
    public NextagContext(DbContextOptions<NextagContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activo> Activos { get; set; }

    public virtual DbSet<ActivoAsignacion> ActivoAsignacions { get; set; }

    public virtual DbSet<ActivoEtiquetum> ActivoEtiqueta { get; set; }

    public virtual DbSet<ActivoMovimiento> ActivoMovimientos { get; set; }

    public virtual DbSet<ArchivoAdjunto> ArchivoAdjuntos { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetSystem> AspNetSystems { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<CategoriaActivo> CategoriaActivos { get; set; }

    public virtual DbSet<CategoriaTicket> CategoriaTickets { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<EquipoSoporte> EquipoSoportes { get; set; }

    public virtual DbSet<EquipoSoporteIntegrante> EquipoSoporteIntegrantes { get; set; }

    public virtual DbSet<EstadoActivo> EstadoActivos { get; set; }

    public virtual DbSet<EstadoOrdenTrabajo> EstadoOrdenTrabajos { get; set; }

    public virtual DbSet<EstadoTicket> EstadoTickets { get; set; }

    public virtual DbSet<EtiquetaConfiguracion> EtiquetaConfiguracions { get; set; }

    public virtual DbSet<EtiquetaLectura> EtiquetaLecturas { get; set; }

    public virtual DbSet<Etiquetum> Etiqueta { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<ModeloActivo> ModeloActivos { get; set; }

    public virtual DbSet<OrdenTrabajo> OrdenTrabajos { get; set; }

    public virtual DbSet<OrdenTrabajoActividad> OrdenTrabajoActividads { get; set; }

    public virtual DbSet<PrioridadTicket> PrioridadTickets { get; set; }

    public virtual DbSet<Proveedor> Proveedors { get; set; }

    public virtual DbSet<ProveedorContacto> ProveedorContactos { get; set; }

    public virtual DbSet<ProveedorTipo> ProveedorTipos { get; set; }

    public virtual DbSet<Responsable> Responsables { get; set; }

    public virtual DbSet<Sucursal> Sucursals { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketAdjuntoToken> TicketAdjuntoTokens { get; set; }

    public virtual DbSet<TicketAsignacion> TicketAsignacions { get; set; }

    public virtual DbSet<TicketEstadoHistorial> TicketEstadoHistorials { get; set; }

    public virtual DbSet<TicketFolioContador> TicketFolioContadors { get; set; }

    public virtual DbSet<TicketSeguimiento> TicketSeguimientos { get; set; }

    public virtual DbSet<TipoMovimientoActivo> TipoMovimientoActivos { get; set; }

    public virtual DbSet<TipoProveedor> TipoProveedors { get; set; }

    public virtual DbSet<TipoUbicacion> TipoUbicacions { get; set; }

    public virtual DbSet<Ubicacion> Ubicacions { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuarioEmpresa> UsuarioEmpresas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activo>(entity =>
        {
            entity.ToTable("Activo");

            entity.HasIndex(e => new { e.EmpresaId, e.CategoriaActivoId }, "IX_Activo_Empresa_Categoria");

            entity.HasIndex(e => new { e.EmpresaId, e.EstadoActivoId }, "IX_Activo_Empresa_Estado");

            entity.HasIndex(e => new { e.EmpresaId, e.NumeroSerie }, "IX_Activo_Empresa_NumeroSerie");

            entity.HasIndex(e => new { e.EmpresaId, e.ProveedorId }, "IX_Activo_Empresa_Proveedor");

            entity.HasIndex(e => new { e.EmpresaId, e.SucursalId, e.UbicacionId }, "IX_Activo_Empresa_Ubicacion");

            entity.HasIndex(e => new { e.EmpresaId, e.Codigo }, "UQ_Activo_Empresa_Codigo").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.Id }, "UQ_Activo_Empresa_Id").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Codigo).HasMaxLength(50);
            entity.Property(e => e.CostoAdquisicion).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.EstadoActivoId).HasDefaultValueSql("((1))");
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaAdquisicion).HasColumnType("date");
            entity.Property(e => e.FechaAlta)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FechaBaja).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Moneda)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValueSql("('MXN')")
                .IsFixedLength();
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.NumeroInventario).HasMaxLength(100);
            entity.Property(e => e.NumeroSerie).HasMaxLength(150);
            entity.Property(e => e.Observaciones).HasMaxLength(1000);
            entity.Property(e => e.PropietarioNombre).HasMaxLength(200);
            entity.Property(e => e.TipoPropiedad)
                .HasMaxLength(20)
                .HasDefaultValueSql("('PROPIO')");

            entity.HasOne(d => d.Empresa).WithMany(p => p.Activos)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Activo_Empresa");

            entity.HasOne(d => d.EstadoActivo).WithMany(p => p.Activos)
                .HasForeignKey(d => d.EstadoActivoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Activo_EstadoActivo");

            entity.HasOne(d => d.CategoriaActivo).WithMany(p => p.Activos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.CategoriaActivoId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Activo_Categoria");

            entity.HasOne(d => d.Marca).WithMany(p => p.Activos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.MarcaId })
                .HasConstraintName("FK_Activo_Marca");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.ActivoProveedors)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.PropietarioProveedorId })
                .HasConstraintName("FK_Activo_PropietarioProveedor");

            entity.HasOne(d => d.ProveedorNavigation).WithMany(p => p.ActivoProveedorNavigations)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.ProveedorId })
                .HasConstraintName("FK_Activo_Proveedor");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Activos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.SucursalId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Activo_Sucursal");

            entity.HasOne(d => d.Ubicacion).WithMany(p => p.Activos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.SucursalId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.SucursalId, d.UbicacionId })
                .HasConstraintName("FK_Activo_Ubicacion");

            entity.HasOne(d => d.ModeloActivo).WithMany(p => p.Activos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.CategoriaActivoId, p.MarcaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.CategoriaActivoId, d.MarcaId, d.ModeloActivoId })
                .HasConstraintName("FK_Activo_Modelo");
        });

        modelBuilder.Entity<ActivoAsignacion>(entity =>
        {
            entity.ToTable("ActivoAsignacion");

            entity.HasIndex(e => new { e.EmpresaId, e.FechaAsignacion }, "IX_ActivoAsignacion_Empresa_Fecha").IsDescending(false, true);

            entity.HasIndex(e => new { e.EmpresaId, e.ActivoId, e.FechaAsignacion }, "IX_ActivoAsignacion_HistorialActivo").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.EmpresaId, e.ResponsableId, e.FechaDevolucion }, "IX_ActivoAsignacion_Responsable");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.CondicionDevolucion).HasMaxLength(100);
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaAsignacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FechaDevolucion).HasPrecision(3);
            entity.Property(e => e.MotivoAsignacion).HasMaxLength(300);
            entity.Property(e => e.MotivoDevolucion).HasMaxLength(300);
            entity.Property(e => e.ObservacionesAsignacion).HasMaxLength(1000);
            entity.Property(e => e.ObservacionesDevolucion).HasMaxLength(1000);

            entity.HasOne(d => d.Empresa).WithMany(p => p.ActivoAsignacions)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActivoAsignacion_Empresa");

            entity.HasOne(d => d.Activo).WithMany(p => p.ActivoAsignacions)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.ActivoId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActivoAsignacion_Activo");

            entity.HasOne(d => d.Responsable).WithMany(p => p.ActivoAsignacions)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.ResponsableId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActivoAsignacion_Responsable");
        });

        modelBuilder.Entity<ActivoEtiquetum>(entity =>
        {
            entity.HasIndex(e => new { e.EmpresaId, e.EtiquetaId, e.FechaVinculacion }, "IX_ActivoEtiqueta_Etiqueta_Historial").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.EmpresaId, e.ActivoId, e.FechaVinculacion }, "IX_ActivoEtiqueta_Historial").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.EmpresaId, e.EtiquetaId, e.FechaDesvinculacion }, "UQ_ActivoEtiqueta_Etiqueta").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.ActivoId, e.FechaDesvinculacion }, "UQ_ActivoEtiqueta_Vigente")
                .IsUnique()
                .HasFilter("([FechaDesvinculacion] IS NULL)");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.FechaDesvinculacion).HasPrecision(3);
            entity.Property(e => e.FechaVinculacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.MotivoDesvinculacion).HasMaxLength(300);

            entity.HasOne(d => d.Activo).WithMany(p => p.ActivoEtiqueta)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.ActivoId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActivoEtiqueta_Activo");

            entity.HasOne(d => d.E).WithMany(p => p.ActivoEtiqueta)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.EtiquetaId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActivoEtiqueta_Etiqueta");
        });

        modelBuilder.Entity<ActivoMovimiento>(entity =>
        {
            entity.ToTable("ActivoMovimiento");

            entity.HasIndex(e => new { e.EmpresaId, e.EsDestinoExterno, e.FechaMovimiento }, "IX_ActivoMovimiento_DestinoExterno").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.EmpresaId, e.FechaMovimiento }, "IX_ActivoMovimiento_Empresa_Fecha").IsDescending(false, true);

            entity.HasIndex(e => new { e.EmpresaId, e.TipoMovimientoActivoId, e.FechaMovimiento }, "IX_ActivoMovimiento_Empresa_Tipo").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.EmpresaId, e.ActivoId, e.FechaMovimiento }, "IX_ActivoMovimiento_HistorialActivo").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.EmpresaId, e.ActivoId, e.MovimientoOrigenId }, "IX_ActivoMovimiento_MovimientoOrigen").HasFilter("([MovimientoOrigenId] IS NOT NULL)");

            entity.HasIndex(e => new { e.EmpresaId, e.TipoReferencia, e.ReferenciaId }, "IX_ActivoMovimiento_Referencia").HasFilter("([ReferenciaId] IS NOT NULL)");

            entity.HasIndex(e => new { e.EmpresaId, e.ActivoId, e.Id }, "UQ_ActivoMovimiento_Empresa_Activo_Id").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.DestinoExternoDireccion).HasMaxLength(500);
            entity.Property(e => e.DestinoExternoNombre).HasMaxLength(200);
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaAnulacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FechaMovimiento)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Motivo).HasMaxLength(300);
            entity.Property(e => e.MotivoAnulacion).HasMaxLength(500);
            entity.Property(e => e.Observaciones).HasMaxLength(1000);
            entity.Property(e => e.OrigenExternoDireccion).HasMaxLength(500);
            entity.Property(e => e.OrigenExternoNombre).HasMaxLength(200);
            entity.Property(e => e.SucursalDestinoNombre).HasMaxLength(200);
            entity.Property(e => e.SucursalOrigenNombre).HasMaxLength(200);
            entity.Property(e => e.TipoReferencia)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.UbicacionDestinoNombre).HasMaxLength(200);
            entity.Property(e => e.UbicacionOrigenNombre).HasMaxLength(200);

            entity.HasOne(d => d.Empresa).WithMany(p => p.ActivoMovimientos)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActivoMovimiento_Empresa");

            entity.HasOne(d => d.EstadoActivoDestino).WithMany(p => p.ActivoMovimientoEstadoActivoDestinos)
                .HasForeignKey(d => d.EstadoActivoDestinoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActivoMovimiento_EstadoDestino");

            entity.HasOne(d => d.EstadoActivoOrigen).WithMany(p => p.ActivoMovimientoEstadoActivoOrigens)
                .HasForeignKey(d => d.EstadoActivoOrigenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActivoMovimiento_EstadoOrigen");

            entity.HasOne(d => d.TipoMovimientoActivo).WithMany(p => p.ActivoMovimientos)
                .HasForeignKey(d => d.TipoMovimientoActivoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActivoMovimiento_Tipo");

            entity.HasOne(d => d.Activo).WithMany(p => p.ActivoMovimientos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.ActivoId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActivoMovimiento_Activo");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.ActivoMovimientos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.ProveedorDestinoId })
                .HasConstraintName("FK_ActivoMovimiento_ProveedorDestino");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.ActivoMovimientoSucursals)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.SucursalDestinoId })
                .HasConstraintName("FK_ActivoMovimiento_SucursalDestino");

            entity.HasOne(d => d.SucursalNavigation).WithMany(p => p.ActivoMovimientoSucursalNavigations)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.SucursalOrigenId })
                .HasConstraintName("FK_ActivoMovimiento_SucursalOrigen");

            entity.HasOne(d => d.ActivoMovimientoNavigation).WithMany(p => p.InverseActivoMovimientoNavigation)
                .HasPrincipalKey(p => new { p.EmpresaId, p.ActivoId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.ActivoId, d.MovimientoOrigenId })
                .HasConstraintName("FK_ActivoMovimiento_MovimientoOrigen");

            entity.HasOne(d => d.Ubicacion).WithMany(p => p.ActivoMovimientoUbicacions)
                .HasPrincipalKey(p => new { p.EmpresaId, p.SucursalId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.SucursalDestinoId, d.UbicacionDestinoId })
                .HasConstraintName("FK_ActivoMovimiento_UbicacionDestino");

            entity.HasOne(d => d.UbicacionNavigation).WithMany(p => p.ActivoMovimientoUbicacionNavigations)
                .HasPrincipalKey(p => new { p.EmpresaId, p.SucursalId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.SucursalOrigenId, d.UbicacionOrigenId })
                .HasConstraintName("FK_ActivoMovimiento_UbicacionOrigen");
        });

        modelBuilder.Entity<ArchivoAdjunto>(entity =>
        {
            entity.ToTable("ArchivoAdjunto");

            entity.HasIndex(e => new { e.EmpresaId, e.TicketId }, "IX_ArchivoAdjunto_Empresa_Ticket_Activo").HasFilter("([Eliminado]=(0))");

            entity.HasIndex(e => new { e.EmpresaId, e.OrdenTrabajoId }, "IX_ArchivoAdjunto_OT");

            entity.HasIndex(e => new { e.EmpresaId, e.TicketSeguimientoId }, "IX_ArchivoAdjunto_Seguimiento");

            entity.HasIndex(e => new { e.EmpresaId, e.TicketId }, "IX_ArchivoAdjunto_Ticket");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.EstadoAlmacenamiento)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValueSql("('DISPONIBLE')");
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FechaEliminacion).HasPrecision(3);
            entity.Property(e => e.HashSha256)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.MimeType).HasMaxLength(120);
            entity.Property(e => e.NombreAlmacenado).HasMaxLength(255);
            entity.Property(e => e.NombreOriginal).HasMaxLength(255);
            entity.Property(e => e.RutaStorage).HasMaxLength(1000);

            entity.HasOne(d => d.Empresa).WithMany(p => p.ArchivoAdjuntos)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArchivoAdjunto_Empresa");

            entity.HasOne(d => d.OrdenTrabajo).WithMany(p => p.ArchivoAdjuntos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.OrdenTrabajoId })
                .HasConstraintName("FK_ArchivoAdjunto_OT");

            entity.HasOne(d => d.Ticket).WithMany(p => p.ArchivoAdjuntos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.TicketId })
                .HasConstraintName("FK_ArchivoAdjunto_Ticket");

            entity.HasOne(d => d.TicketSeguimiento).WithMany(p => p.ArchivoAdjuntos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.TicketSeguimientoId })
                .HasConstraintName("FK_ArchivoAdjunto_Seguimiento");
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);

            entity.HasOne(d => d.Sistema).WithMany(p => p.AspNetRoles)
                .HasForeignKey(d => d.SistemaId)
                .HasConstraintName("FK_AspNetRoles_AspNetSystem");
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.Property(e => e.RoleId).HasMaxLength(450);

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetSystem>(entity =>
        {
            entity.ToTable("AspNetSystem");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.Nombre).HasMaxLength(500);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.Property(e => e.Apellidos).HasMaxLength(250);
            entity.Property(e => e.Avatar).HasMaxLength(450);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Nombre).HasMaxLength(250);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<CategoriaActivo>(entity =>
        {
            entity.ToTable("CategoriaActivo");

            entity.HasIndex(e => new { e.EmpresaId, e.Activo, e.Nombre }, "IX_CategoriaActivo_Empresa_Activo_Nombre");

            entity.HasIndex(e => new { e.EmpresaId, e.CategoriaPadreId, e.Activo, e.Nombre }, "IX_CategoriaActivo_Empresa_Padre_Activo_Nombre");

            entity.HasIndex(e => new { e.EmpresaId, e.Clave }, "UQ_CategoriaActivo_Empresa_Clave").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.Id }, "UQ_CategoriaActivo_Empresa_Id").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Nombre).HasMaxLength(200);

            entity.HasOne(d => d.Empresa).WithMany(p => p.CategoriaActivos)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CategoriaActivo_Empresa");

            entity.HasOne(d => d.CategoriaActivoNavigation).WithMany(p => p.InverseCategoriaActivoNavigation)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.CategoriaPadreId })
                .HasConstraintName("FK_CategoriaActivo_PadreMismaEmpresa");
        });

        modelBuilder.Entity<CategoriaTicket>(entity =>
        {
            entity.ToTable("CategoriaTicket");

            entity.HasIndex(e => new { e.EmpresaId, e.Clave }, "UQ_CategoriaTicket_Empresa_Clave").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.Id }, "UQ_CategoriaTicket_Empresa_Id").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Clave).HasMaxLength(40);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Nombre).HasMaxLength(150);

            entity.HasOne(d => d.Empresa).WithMany(p => p.CategoriaTickets)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CategoriaTicket_Empresa");
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.ToTable("Empresa");

            entity.HasIndex(e => new { e.Activo, e.Nombre }, "IX_Empresa_Activo_Nombre");

            entity.HasIndex(e => e.Clave, "UQ_Empresa_Clave").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.Correo).HasMaxLength(256);
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.RazonSocial).HasMaxLength(250);
            entity.Property(e => e.Rfc).HasMaxLength(20);
            entity.Property(e => e.SitioWeb).HasMaxLength(250);
            entity.Property(e => e.Telefono).HasMaxLength(30);
        });

        modelBuilder.Entity<EquipoSoporte>(entity =>
        {
            entity.ToTable("EquipoSoporte");

            entity.HasIndex(e => new { e.EmpresaId, e.Activo }, "IX_EquipoSoporte_Empresa_Activo");

            entity.HasIndex(e => new { e.EmpresaId, e.Clave }, "UQ_EquipoSoporte_Empresa_Clave").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.Id }, "UQ_EquipoSoporte_Empresa_Id").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Clave).HasMaxLength(40);
            entity.Property(e => e.CorreoContacto).HasMaxLength(200);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.Tipo)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValueSql("('INTERNO')");

            entity.HasOne(d => d.Empresa).WithMany(p => p.EquipoSoportes)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EquipoSoporte_Empresa");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.EquipoSoportes)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.ProveedorId })
                .HasConstraintName("FK_EquipoSoporte_Proveedor");
        });

        modelBuilder.Entity<EquipoSoporteIntegrante>(entity =>
        {
            entity.ToTable("EquipoSoporteIntegrante");

            entity.HasIndex(e => new { e.EmpresaId, e.UsuarioId, e.Activo }, "IX_EquipoIntegrante_Empresa_Usuario");

            entity.HasIndex(e => new { e.EmpresaId, e.Id }, "UQ_EquipoIntegrante_Empresa_Id").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.EquipoSoporteId, e.UsuarioId }, "UQ_EquipoIntegrante_Equipo_Usuario").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Usuario).WithMany(p => p.EquipoSoporteIntegrantes)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EquipoIntegrante_Usuario");

            entity.HasOne(d => d.E).WithMany(p => p.EquipoSoporteIntegrantes)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.EquipoSoporteId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EquipoIntegrante_Equipo");
        });

        modelBuilder.Entity<EstadoActivo>(entity =>
        {
            entity.ToTable("EstadoActivo");

            entity.HasIndex(e => e.Clave, "UQ_EstadoActivo_Clave").IsUnique();

            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<EstadoOrdenTrabajo>(entity =>
        {
            entity.ToTable("EstadoOrdenTrabajo");

            entity.HasIndex(e => e.Clave, "UQ_EstadoOrdenTrabajo_Clave").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Clave)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Nombre).HasMaxLength(80);
        });

        modelBuilder.Entity<EstadoTicket>(entity =>
        {
            entity.ToTable("EstadoTicket");

            entity.HasIndex(e => e.Clave, "UQ_EstadoTicket_Clave").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Clave)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Nombre).HasMaxLength(80);
        });

        modelBuilder.Entity<EtiquetaConfiguracion>(entity =>
        {
            entity.ToTable("EtiquetaConfiguracion");

            entity.HasIndex(e => e.EmpresaId, "UQ_EtiquetaConfiguracion_Empresa").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.MostrarCategoria)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.MostrarCodigoActivo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.MostrarDatosEmpresa)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.MostrarEstadoActivo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.MostrarMarcaModelo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.MostrarNombreActivo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.MostrarNumeroInventario)
                .IsRequired()
                .HasDefaultValueSql("((1))");

            entity.HasOne(d => d.Empresa).WithOne(p => p.EtiquetaConfiguracion)
                .HasForeignKey<EtiquetaConfiguracion>(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EtiquetaConfiguracion_Empresa");
        });

        modelBuilder.Entity<EtiquetaLectura>(entity =>
        {
            entity.ToTable("EtiquetaLectura");

            entity.HasIndex(e => new { e.EmpresaId, e.ActivoId, e.FechaLectura }, "IX_EtiquetaLectura_Activo_Fecha").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.EmpresaId, e.FechaLectura }, "IX_EtiquetaLectura_Empresa_Fecha").IsDescending(false, true);

            entity.HasIndex(e => new { e.EmpresaId, e.EtiquetaId, e.FechaLectura }, "IX_EtiquetaLectura_Etiqueta_Fecha").IsDescending(false, false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.FechaLectura)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Ip)
                .HasMaxLength(45)
                .IsUnicode(false);
            entity.Property(e => e.Resultado)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TipoLectura)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValueSql("('DESCONOCIDO')");
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasOne(d => d.Activo).WithMany(p => p.EtiquetaLecturas)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.ActivoId })
                .HasConstraintName("FK_EtiquetaLectura_Activo");

            entity.HasOne(d => d.E).WithMany(p => p.EtiquetaLecturas)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.EtiquetaId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EtiquetaLectura_Etiqueta");
        });

        modelBuilder.Entity<Etiquetum>(entity =>
        {
            entity.HasIndex(e => new { e.EmpresaId, e.Anulada }, "IX_Etiqueta_Empresa_Anulada");

            entity.HasIndex(e => e.UidNfc, "IX_Etiqueta_UidNfc").HasFilter("([UidNfc] IS NOT NULL)");

            entity.HasIndex(e => e.Codigo, "UQ_Etiqueta_Codigo").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.Id }, "UQ_Etiqueta_Empresa_Id").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.UidNfc }, "UQ_Etiqueta_Empresa_UidNfc")
                .IsUnique()
                .HasFilter("([UidNfc] IS NOT NULL)");

            entity.HasIndex(e => e.TokenPublico, "UQ_Etiqueta_TokenPublico").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Codigo).HasMaxLength(50);
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaAnulacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.MotivoAnulacion).HasMaxLength(300);
            entity.Property(e => e.PublicaHabilitada)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.TieneNfc)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.TieneQr)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.TokenPublico).HasMaxLength(80);
            entity.Property(e => e.UidNfc).HasMaxLength(100);

            entity.HasOne(d => d.Empresa).WithMany(p => p.Etiqueta)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Etiqueta_Empresa");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.ToTable("Marca");

            entity.HasIndex(e => new { e.EmpresaId, e.Activo, e.Nombre }, "IX_Marca_Empresa_Activo_Nombre");

            entity.HasIndex(e => new { e.EmpresaId, e.Clave }, "UQ_Marca_Empresa_Clave").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.Id }, "UQ_Marca_Empresa_Id").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Nombre).HasMaxLength(150);

            entity.HasOne(d => d.Empresa).WithMany(p => p.Marcas)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Marca_Empresa");
        });

        modelBuilder.Entity<ModeloActivo>(entity =>
        {
            entity.ToTable("ModeloActivo");

            entity.HasIndex(e => new { e.EmpresaId, e.CategoriaActivoId, e.Activo, e.Nombre }, "IX_ModeloActivo_Empresa_Categoria_Activo_Nombre");

            entity.HasIndex(e => new { e.EmpresaId, e.MarcaId, e.Activo, e.Nombre }, "IX_ModeloActivo_Empresa_Marca_Activo_Nombre");

            entity.HasIndex(e => new { e.EmpresaId, e.CategoriaActivoId, e.MarcaId, e.Id }, "UQ_ModeloActivo_Empresa_Categoria_Marca_Id").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.Id }, "UQ_ModeloActivo_Empresa_Id").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.MarcaId, e.Clave }, "UQ_ModeloActivo_Empresa_Marca_Clave").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Nombre).HasMaxLength(200);

            entity.HasOne(d => d.Empresa).WithMany(p => p.ModeloActivos)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ModeloActivo_Empresa");

            entity.HasOne(d => d.CategoriaActivo).WithMany(p => p.ModeloActivos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.CategoriaActivoId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ModeloActivo_EmpresaCategoria");

            entity.HasOne(d => d.Marca).WithMany(p => p.ModeloActivos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.MarcaId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ModeloActivo_EmpresaMarca");
        });

        modelBuilder.Entity<OrdenTrabajo>(entity =>
        {
            entity.ToTable("OrdenTrabajo");

            entity.HasIndex(e => new { e.EmpresaId, e.ActivoId, e.FechaCreacion }, "IX_OrdenTrabajo_Activo").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.EmpresaId, e.EquipoSoporteId, e.EstadoOrdenTrabajoId }, "IX_OrdenTrabajo_Equipo_Estado");

            entity.HasIndex(e => new { e.EmpresaId, e.TicketId }, "IX_OrdenTrabajo_Ticket");

            entity.HasIndex(e => new { e.EmpresaId, e.Id }, "UQ_OrdenTrabajo_Empresa_Id").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.EstadoOrdenTrabajoId).HasDefaultValueSql("((1))");
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FechaFin).HasPrecision(3);
            entity.Property(e => e.FechaInicio).HasPrecision(3);
            entity.Property(e => e.FechaProgramada).HasPrecision(3);
            entity.Property(e => e.Origen)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValueSql("('MANUAL')");
            entity.Property(e => e.Titulo).HasMaxLength(200);

            entity.HasOne(d => d.Empresa).WithMany(p => p.OrdenTrabajos)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrdenTrabajo_Empresa");

            entity.HasOne(d => d.EstadoOrdenTrabajo).WithMany(p => p.OrdenTrabajos)
                .HasForeignKey(d => d.EstadoOrdenTrabajoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrdenTrabajo_Estado");

            entity.HasOne(d => d.UsuarioTecnico).WithMany(p => p.OrdenTrabajos)
                .HasForeignKey(d => d.UsuarioTecnicoId)
                .HasConstraintName("FK_OrdenTrabajo_Tecnico");

            entity.HasOne(d => d.Activo).WithMany(p => p.OrdenTrabajos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.ActivoId })
                .HasConstraintName("FK_OrdenTrabajo_Activo");

            entity.HasOne(d => d.E).WithMany(p => p.OrdenTrabajos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.EquipoSoporteId })
                .HasConstraintName("FK_OrdenTrabajo_Equipo");

            entity.HasOne(d => d.Ticket).WithMany(p => p.OrdenTrabajos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.TicketId })
                .HasConstraintName("FK_OrdenTrabajo_Ticket");
        });

        modelBuilder.Entity<OrdenTrabajoActividad>(entity =>
        {
            entity.ToTable("OrdenTrabajoActividad");

            entity.HasIndex(e => new { e.EmpresaId, e.OrdenTrabajoId, e.FechaCreacion }, "IX_OTActividad_Orden_Fecha");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Descripcion).HasMaxLength(2000);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FechaFin).HasPrecision(3);
            entity.Property(e => e.FechaInicio).HasPrecision(3);

            entity.HasOne(d => d.UsuarioTecnico).WithMany(p => p.OrdenTrabajoActividads)
                .HasForeignKey(d => d.UsuarioTecnicoId)
                .HasConstraintName("FK_OTActividad_Tecnico");

            entity.HasOne(d => d.OrdenTrabajo).WithMany(p => p.OrdenTrabajoActividads)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.OrdenTrabajoId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OTActividad_Orden");
        });

        modelBuilder.Entity<PrioridadTicket>(entity =>
        {
            entity.ToTable("PrioridadTicket");

            entity.HasIndex(e => e.Clave, "UQ_PrioridadTicket_Clave").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Clave)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Nombre).HasMaxLength(60);
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.ToTable("Proveedor");

            entity.HasIndex(e => new { e.EmpresaId, e.Activo, e.Nombre }, "IX_Proveedor_Empresa_Activo_Nombre");

            entity.HasIndex(e => new { e.EmpresaId, e.Clave }, "UQ_Proveedor_Empresa_Clave").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.Id }, "UQ_Proveedor_Empresa_Id").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.CodigoPostal).HasMaxLength(15);
            entity.Property(e => e.Colonia).HasMaxLength(150);
            entity.Property(e => e.Correo).HasMaxLength(256);
            entity.Property(e => e.Direccion).HasMaxLength(300);
            entity.Property(e => e.Estado).HasMaxLength(150);
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Municipio).HasMaxLength(150);
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Observaciones).HasMaxLength(1000);
            entity.Property(e => e.Pais).HasMaxLength(100);
            entity.Property(e => e.RazonSocial).HasMaxLength(250);
            entity.Property(e => e.Rfc).HasMaxLength(13);
            entity.Property(e => e.SitioWeb).HasMaxLength(300);
            entity.Property(e => e.Telefono).HasMaxLength(30);

            entity.HasOne(d => d.Empresa).WithMany(p => p.Proveedors)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Proveedor_Empresa");
        });

        modelBuilder.Entity<ProveedorContacto>(entity =>
        {
            entity.ToTable("ProveedorContacto");

            entity.HasIndex(e => new { e.EmpresaId, e.ProveedorId, e.Activo, e.Nombre }, "IX_ProveedorContacto_Proveedor");

            entity.HasIndex(e => new { e.EmpresaId, e.ProveedorId, e.EsPrincipal }, "UQ_ProveedorContacto_Principal")
                .IsUnique()
                .HasFilter("([EsPrincipal]=(1) AND [Activo]=(1))");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Cargo).HasMaxLength(150);
            entity.Property(e => e.Celular).HasMaxLength(30);
            entity.Property(e => e.Correo).HasMaxLength(256);
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Telefono).HasMaxLength(30);

            entity.HasOne(d => d.Proveedor).WithMany(p => p.ProveedorContactos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.ProveedorId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProveedorContacto_Proveedor");
        });

        modelBuilder.Entity<ProveedorTipo>(entity =>
        {
            entity.ToTable("ProveedorTipo");

            entity.HasIndex(e => new { e.TipoProveedorId, e.EmpresaId }, "IX_ProveedorTipo_TipoProveedor");

            entity.HasIndex(e => new { e.EmpresaId, e.ProveedorId, e.TipoProveedorId }, "UQ_ProveedorTipo_Proveedor_Tipo").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.TipoProveedor).WithMany(p => p.ProveedorTipos)
                .HasForeignKey(d => d.TipoProveedorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProveedorTipo_TipoProveedor");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.ProveedorTipos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.ProveedorId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProveedorTipo_Proveedor");
        });

        modelBuilder.Entity<Responsable>(entity =>
        {
            entity.ToTable("Responsable");

            entity.HasIndex(e => new { e.EmpresaId, e.AreaResponsableId }, "IX_Responsable_Empresa_Area").HasFilter("([AreaResponsableId] IS NOT NULL)");

            entity.HasIndex(e => new { e.EmpresaId, e.SucursalId }, "IX_Responsable_Empresa_Sucursal");

            entity.HasIndex(e => new { e.EmpresaId, e.Tipo, e.Activo, e.Nombre }, "IX_Responsable_Empresa_Tipo_Activo_Nombre");

            entity.HasIndex(e => new { e.EmpresaId, e.Id }, "UQ_Responsable_Empresa_Id").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.Tipo, e.Codigo }, "UQ_Responsable_Empresa_Tipo_Codigo")
                .IsUnique()
                .HasFilter("([Codigo] IS NOT NULL)");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Codigo).HasMaxLength(50);
            entity.Property(e => e.Contacto).HasMaxLength(150);
            entity.Property(e => e.Correo).HasMaxLength(256);
            entity.Property(e => e.Departamento).HasMaxLength(150);
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Observaciones).HasMaxLength(500);
            entity.Property(e => e.Puesto).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(30);
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Empresa).WithMany(p => p.Responsables)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Responsable_Empresa");

            entity.HasOne(d => d.ResponsableNavigation).WithMany(p => p.InverseResponsableNavigation)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.AreaResponsableId })
                .HasConstraintName("FK_Responsable_Area");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Responsables)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.SucursalId })
                .HasConstraintName("FK_Responsable_Sucursal");
        });

        modelBuilder.Entity<Sucursal>(entity =>
        {
            entity.ToTable("Sucursal");

            entity.HasIndex(e => new { e.EmpresaId, e.Activo, e.Nombre }, "IX_Sucursal_EmpresaId_Activo_Nombre");

            entity.HasIndex(e => new { e.EmpresaId, e.Clave }, "UQ_Sucursal_EmpresaId_Clave").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.Id }, "UQ_Sucursal_Empresa_Id").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.CodigoPostal).HasMaxLength(15);
            entity.Property(e => e.Colonia).HasMaxLength(150);
            entity.Property(e => e.Correo).HasMaxLength(256);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Direccion).HasMaxLength(300);
            entity.Property(e => e.Estado).HasMaxLength(150);
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Municipio).HasMaxLength(150);
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Pais).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(30);

            entity.HasOne(d => d.Empresa).WithMany(p => p.Sucursals)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sucursal_Empresa");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Ticket");

            entity.HasIndex(e => new { e.EmpresaId, e.ActivoId, e.FechaApertura }, "IX_Ticket_Empresa_Activo_Fecha").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.EmpresaId, e.EstadoTicketId, e.FechaApertura }, "IX_Ticket_Empresa_Estado_Fecha").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.EmpresaId, e.PrioridadTicketId, e.FechaApertura }, "IX_Ticket_Empresa_Prioridad").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.EmpresaId, e.Folio }, "UQ_Ticket_Empresa_Folio").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.Id }, "UQ_Ticket_Empresa_Id").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.Numero }, "UQ_Ticket_Empresa_Numero").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.EstadoTicketId).HasDefaultValueSql("((1))");
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaApertura)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FechaCancelacion).HasPrecision(3);
            entity.Property(e => e.FechaCierre).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FechaPrimeraAtencion).HasPrecision(3);
            entity.Property(e => e.FechaResolucion).HasPrecision(3);
            entity.Property(e => e.Folio).HasMaxLength(30);
            entity.Property(e => e.MotivoCancelacion).HasMaxLength(500);
            entity.Property(e => e.Origen)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValueSql("('INTERNO')");
            entity.Property(e => e.PrioridadTicketId).HasDefaultValueSql("((2))");
            entity.Property(e => e.SolicitanteCorreo).HasMaxLength(200);
            entity.Property(e => e.SolicitanteNombre).HasMaxLength(200);
            entity.Property(e => e.SolicitanteTelefono).HasMaxLength(40);
            entity.Property(e => e.Titulo).HasMaxLength(200);

            entity.HasOne(d => d.Empresa).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_Empresa");

            entity.HasOne(d => d.EstadoTicket).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.EstadoTicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_Estado");

            entity.HasOne(d => d.PrioridadTicket).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.PrioridadTicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_Prioridad");

            entity.HasOne(d => d.SolicitanteUsuario).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.SolicitanteUsuarioId)
                .HasConstraintName("FK_Ticket_SolicitanteUsuario");

            entity.HasOne(d => d.Activo).WithMany(p => p.Tickets)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.ActivoId })
                .HasConstraintName("FK_Ticket_Activo");

            entity.HasOne(d => d.CategoriaTicket).WithMany(p => p.Tickets)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.CategoriaTicketId })
                .HasConstraintName("FK_Ticket_Categoria");
        });

        modelBuilder.Entity<TicketAdjuntoToken>(entity =>
        {
            entity.ToTable("TicketAdjuntoToken");

            entity.HasIndex(e => new { e.EmpresaId, e.TicketId }, "IX_TicketAdjuntoToken_Empresa_Ticket");

            entity.HasIndex(e => e.FechaExpiracion, "IX_TicketAdjuntoToken_Expiracion");

            entity.HasIndex(e => e.TokenHash, "UQ_TicketAdjuntoToken_TokenHash").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FechaExpiracion).HasPrecision(3);
            entity.Property(e => e.FechaRevocacion).HasPrecision(3);
            entity.Property(e => e.MaximoArchivos).HasDefaultValueSql("((5))");
            entity.Property(e => e.TokenHash)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.UltimaUtilizacion).HasPrecision(3);

            entity.HasOne(d => d.Empresa).WithMany(p => p.TicketAdjuntoTokens)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TicketAdjuntoToken_Empresa");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketAdjuntoTokens)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.TicketId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TicketAdjuntoToken_Ticket");
        });

        modelBuilder.Entity<TicketAsignacion>(entity =>
        {
            entity.ToTable("TicketAsignacion");

            entity.HasIndex(e => new { e.EmpresaId, e.EquipoSoporteId, e.FechaFin }, "IX_TicketAsignacion_Equipo");

            entity.HasIndex(e => new { e.EmpresaId, e.Id }, "UQ_TicketAsignacion_Empresa_Id").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.TicketId, e.FechaFin }, "UX_TicketAsignacion_Vigente")
                .IsUnique()
                .HasFilter("([FechaFin] IS NULL)");

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.FechaAsignacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FechaFin).HasPrecision(3);
            entity.Property(e => e.Motivo).HasMaxLength(300);

            entity.HasOne(d => d.UsuarioTecnico).WithMany(p => p.TicketAsignacions)
                .HasForeignKey(d => d.UsuarioTecnicoId)
                .HasConstraintName("FK_TicketAsignacion_Tecnico");

            entity.HasOne(d => d.E).WithMany(p => p.TicketAsignacions)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.EquipoSoporteId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TicketAsignacion_Equipo");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketAsignacions)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.TicketId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TicketAsignacion_Ticket");
        });

        modelBuilder.Entity<TicketEstadoHistorial>(entity =>
        {
            entity.ToTable("TicketEstadoHistorial");

            entity.HasIndex(e => new { e.EmpresaId, e.TicketId, e.FechaCambio }, "IX_TicketEstadoHistorial_Ticket_Fecha").IsDescending(false, false, true);

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.FechaCambio)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Motivo).HasMaxLength(500);

            entity.HasOne(d => d.EstadoAnterior).WithMany(p => p.TicketEstadoHistorialEstadoAnteriors)
                .HasForeignKey(d => d.EstadoAnteriorId)
                .HasConstraintName("FK_TicketEstadoHistorial_Anterior");

            entity.HasOne(d => d.EstadoNuevo).WithMany(p => p.TicketEstadoHistorialEstadoNuevos)
                .HasForeignKey(d => d.EstadoNuevoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TicketEstadoHistorial_Nuevo");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketEstadoHistorials)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.TicketId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TicketEstadoHistorial_Ticket");
        });

        modelBuilder.Entity<TicketFolioContador>(entity =>
        {
            entity.HasKey(e => e.EmpresaId);

            entity.ToTable("TicketFolioContador");

            entity.Property(e => e.EmpresaId).ValueGeneratedNever();

            entity.HasOne(d => d.Empresa).WithOne(p => p.TicketFolioContador)
                .HasForeignKey<TicketFolioContador>(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TicketFolio_Empresa");
        });

        modelBuilder.Entity<TicketSeguimiento>(entity =>
        {
            entity.ToTable("TicketSeguimiento");

            entity.HasIndex(e => new { e.EmpresaId, e.TicketId, e.FechaCreacion }, "IX_TicketSeguimiento_Ticket_Fecha");

            entity.HasIndex(e => new { e.EmpresaId, e.Id }, "UQ_TicketSeguimiento_Empresa_Id").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.AutorNombre).HasMaxLength(200);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Tipo)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasDefaultValueSql("('COMENTARIO')");

            entity.HasOne(d => d.AutorUsuario).WithMany(p => p.TicketSeguimientos)
                .HasForeignKey(d => d.AutorUsuarioId)
                .HasConstraintName("FK_TicketSeguimiento_Autor");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketSeguimientos)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.TicketId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TicketSeguimiento_Ticket");
        });

        modelBuilder.Entity<TipoMovimientoActivo>(entity =>
        {
            entity.ToTable("TipoMovimientoActivo");

            entity.HasIndex(e => e.Clave, "UQ_TipoMovimientoActivo_Clave").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Clave)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<TipoProveedor>(entity =>
        {
            entity.ToTable("TipoProveedor");

            entity.HasIndex(e => e.Clave, "UQ_TipoProveedor_Clave").IsUnique();

            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.Nombre).HasMaxLength(150);
        });

        modelBuilder.Entity<TipoUbicacion>(entity =>
        {
            entity.ToTable("TipoUbicacion");

            entity.HasIndex(e => new { e.EmpresaId, e.Activo, e.Nombre }, "IX_TipoUbicacion_Empresa_Activo_Nombre");

            entity.HasIndex(e => new { e.EmpresaId, e.Clave }, "UQ_TipoUbicacion_Empresa_Clave").IsUnique();

            entity.HasIndex(e => new { e.EmpresaId, e.Id }, "UQ_TipoUbicacion_Empresa_Id").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Nombre).HasMaxLength(150);

            entity.HasOne(d => d.Empresa).WithMany(p => p.TipoUbicacions)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TipoUbicacion_Empresa");
        });

        modelBuilder.Entity<Ubicacion>(entity =>
        {
            entity.ToTable("Ubicacion");

            entity.HasIndex(e => new { e.EmpresaId, e.SucursalId, e.Activo, e.Nombre }, "IX_Ubicacion_Empresa_Sucursal_Activo_Nombre");

            entity.HasIndex(e => e.UbicacionPadreId, "IX_Ubicacion_Padre");

            entity.HasIndex(e => e.TipoUbicacionId, "IX_Ubicacion_TipoUbicacion");

            entity.HasIndex(e => new { e.EmpresaId, e.SucursalId, e.Id }, "UQ_Ubicacion_Empresa_Sucursal_Id").IsUnique();

            entity.HasIndex(e => new { e.SucursalId, e.Clave }, "UQ_Ubicacion_Sucursal_Clave").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Clave).HasMaxLength(50);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Nombre).HasMaxLength(200);

            entity.HasOne(d => d.Empresa).WithMany(p => p.Ubicacions)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ubicacion_Empresa");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Ubicacions)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.SucursalId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ubicacion_EmpresaSucursal");

            entity.HasOne(d => d.TipoUbicacion).WithMany(p => p.Ubicacions)
                .HasPrincipalKey(p => new { p.EmpresaId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.TipoUbicacionId })
                .HasConstraintName("FK_Ubicacion_EmpresaTipo");

            entity.HasOne(d => d.UbicacionNavigation).WithMany(p => p.InverseUbicacionNavigation)
                .HasPrincipalKey(p => new { p.EmpresaId, p.SucursalId, p.Id })
                .HasForeignKey(d => new { d.EmpresaId, d.SucursalId, d.UbicacionPadreId })
                .HasConstraintName("FK_Ubicacion_PadreMismaSucursal");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_correo_Usuario");

            entity.ToTable("Usuario");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Apellidos).HasMaxLength(200);
            entity.Property(e => e.AspNetUserId).HasMaxLength(450);
            entity.Property(e => e.Celular).HasMaxLength(30);
            entity.Property(e => e.CodigoPais).HasMaxLength(10);
            entity.Property(e => e.DominioPrincipal).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(320);
            entity.Property(e => e.Empresa).HasMaxLength(200);
            entity.Property(e => e.EsAdministrador)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Nombre).HasMaxLength(200);
            entity.Property(e => e.Pais).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(50);

            entity.HasOne(d => d.AspNetUser).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.AspNetUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_correo_Usuario_AspNetUsers");
        });

        modelBuilder.Entity<UsuarioEmpresa>(entity =>
        {
            entity.ToTable("UsuarioEmpresa");

            entity.HasIndex(e => new { e.EmpresaId, e.Activo }, "IX_UsuarioEmpresa_EmpresaId_Activo");

            entity.HasIndex(e => new { e.UsuarioId, e.EmpresaId }, "UQ_UsuarioEmpresa").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.FechaActualizacion).HasPrecision(3);
            entity.Property(e => e.FechaAsignacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Empresa).WithMany(p => p.UsuarioEmpresas)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuarioEmpresa_Empresa");

            entity.HasOne(d => d.Usuario).WithMany(p => p.UsuarioEmpresas)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuarioEmpresa_Usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
