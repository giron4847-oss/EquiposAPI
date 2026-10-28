using EquiposAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EquiposAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Equipo> Equipos => Set<Equipo>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Prestamo> Prestamos => Set<Prestamo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Categoria>(e =>
        {
            e.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
            e.HasIndex(c => c.Nombre).IsUnique();
            e.HasData(
                new Categoria { Id = 1, Nombre = "Laptops" },
                new Categoria { Id = 2, Nombre = "Proyectores" },
                new Categoria { Id = 3, Nombre = "Kits IoT" },
                new Categoria { Id = 4, Nombre = "Tarjetas Arduino" });
        });

        modelBuilder.Entity<Equipo>(e =>
        {
            e.Property(x => x.CodigoInventario).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.CodigoInventario).IsUnique();
            e.Property(x => x.Nombre).IsRequired().HasMaxLength(150);
            e.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Categoria).WithMany(c => c.Equipos)
                .HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.Property(u => u.Username).IsRequired().HasMaxLength(50);
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(150);
            e.Property(u => u.Rol).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<Prestamo>(e =>
        {
            e.HasOne(p => p.Equipo).WithMany(x => x.Prestamos)
                .HasForeignKey(p => p.EquipoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Usuario).WithMany(u => u.Prestamos)
                .HasForeignKey(p => p.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}

