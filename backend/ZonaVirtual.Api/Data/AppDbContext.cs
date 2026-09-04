using Microsoft.EntityFrameworkCore;
using ZonaVirtual.Api.Models;

namespace ZonaVirtual.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Comercio> Comercios => Set<Comercio>();
    public DbSet<UsuarioPagador> UsuariosPagadores => Set<UsuarioPagador>();
    public DbSet<Transaccion> Transacciones => Set<Transaccion>();
    public DbSet<Cuenta> Cuentas => Set<Cuenta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- Comercio ----
        modelBuilder.Entity<Comercio>(e =>
        {
            e.ToTable("Comercios");
            e.HasIndex(c => c.ComercioCodigo).IsUnique();
            e.HasIndex(c => c.ComercioNit).IsUnique();
        });

        // ---- UsuarioPagador ----
        modelBuilder.Entity<UsuarioPagador>(e =>
        {
            e.ToTable("UsuariosPagadores");
            e.HasIndex(u => u.UsuarioIdentificacion).IsUnique();
            e.HasIndex(u => u.UsuarioEmail).IsUnique();
        });

        // ---- Transaccion ----
        modelBuilder.Entity<Transaccion>(e =>
        {
            e.ToTable("Transacciones");
            e.HasIndex(t => t.TransCodigo).IsUnique();
            e.HasIndex(t => t.TransFecha);
            e.HasIndex(t => t.TransEstado);

            e.HasOne(t => t.Comercio)
                .WithMany(c => c.Transacciones)
                .HasForeignKey(t => t.ComercioId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.UsuarioPagador)
                .WithMany(u => u.Transacciones)
                .HasForeignKey(t => t.UsuarioPagadorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Cuenta ----
        modelBuilder.Entity<Cuenta>(e =>
        {
            e.ToTable("Cuentas", t => t.HasCheckConstraint(
                "CK_Cuentas_UnPerfil",
                "([Perfil] = 1 AND [UsuarioPagadorId] IS NOT NULL AND [ComercioId] IS NULL) OR " +
                "([Perfil] = 2 AND [ComercioId] IS NOT NULL AND [UsuarioPagadorId] IS NULL)"));
            e.HasIndex(c => new { c.Perfil, c.Username }).IsUnique();

            e.HasOne(c => c.UsuarioPagador)
                .WithOne(u => u.Cuenta)
                .HasForeignKey<Cuenta>(c => c.UsuarioPagadorId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.Comercio)
                .WithOne(cm => cm.Cuenta)
                .HasForeignKey<Cuenta>(c => c.ComercioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
