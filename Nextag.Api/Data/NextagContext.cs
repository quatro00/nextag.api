using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Nextag.Api.Models.Domain;

namespace Nextag.Api.Data;

public partial class NextagContext : DbContext
{
    public NextagContext()
    {
    }

    public NextagContext(DbContextOptions<NextagContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetSystem> AspNetSystems { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuarioEmpresa> UsuarioEmpresas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
