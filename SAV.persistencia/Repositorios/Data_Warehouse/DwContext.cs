using Microsoft.EntityFrameworkCore;
using SAV.domain.Entities.Data_Warehouse.Dimensions;
using SAV.domain.Entities.Data_Warehouse.Facts;

namespace SAV.persistencia.Repositorios.Data_Warehouse;

public class DwContext : DbContext
{
    public DwContext(DbContextOptions<DwContext> options) : base(options) { }

    public DbSet<DimCliente> DimClientes => Set<DimCliente>();
    public DbSet<DimProducto> DimProductos => Set<DimProducto>();
    public DbSet<DimTiempo> DimTiempos => Set<DimTiempo>();
    public DbSet<DimVendedor> DimVendedores => Set<DimVendedor>();
    public DbSet<DimFuente> DimFuentes => Set<DimFuente>();
    public DbSet<FactVentas> FactVentas => Set<FactVentas>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DimCliente>(e =>
        {
            e.ToTable("DimCliente", "Dimension");
            e.HasKey(e => e.ClienteKey);
            e.Property(e => e.ClienteKey).ValueGeneratedOnAdd();
            e.Property(e => e.Nombre).HasMaxLength(200);
            e.Property(e => e.Pais).HasMaxLength(100);
            e.Property(e => e.Ciudad).HasMaxLength(100);
            e.Property(e => e.Region).HasMaxLength(100);
            e.Property(e => e.Segmento).HasMaxLength(50);
            e.HasOne(e => e.Fuente).WithMany().HasForeignKey(e => e.IdFuente).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimProducto>(e =>
        {
            e.ToTable("DimProducto", "Dimension");
            e.HasKey(e => e.ProductoKey);
            e.Property(e => e.ProductoKey).ValueGeneratedOnAdd();
            e.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
            e.Property(e => e.Categoria).IsRequired().HasMaxLength(100);
            e.Property(e => e.PrecioActual).HasColumnType("decimal(18,2)");
            e.Property(e => e.Marca).HasMaxLength(100);
            e.HasOne(e => e.Fuente).WithMany().HasForeignKey(e => e.IdFuente).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimTiempo>(e =>
        {
            e.ToTable("DimTiempo", "Dimension");
            e.HasKey(e => e.TiempoKey);
            e.Property(e => e.TiempoKey).ValueGeneratedOnAdd();
            e.Property(e => e.NombreMes).HasMaxLength(20);
            e.Property(e => e.DiaSemana).HasMaxLength(20);
            e.HasIndex(e => e.Fecha).IsUnique();
        });

        modelBuilder.Entity<DimVendedor>(e =>
        {
            e.ToTable("DimVendedor", "Dimension");
            e.HasKey(e => e.VendedorKey);
            e.Property(e => e.VendedorKey).ValueGeneratedOnAdd();
            e.Property(e => e.Nombre).HasMaxLength(150);
            e.Property(e => e.Region).HasMaxLength(100);
            e.HasIndex(e => e.VendedorID).IsUnique();
            e.HasOne(e => e.Fuente).WithMany().HasForeignKey(e => e.IdFuente).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DimFuente>(e =>
        {
            e.ToTable("DimFuente", "Dimension");
            e.HasKey(e => e.FuenteKey);
            e.Property(e => e.FuenteKey).ValueGeneratedOnAdd();
            e.Property(e => e.NombreFuente).IsRequired().HasMaxLength(50);
            e.Property(e => e.Tipo).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<FactVentas>(e =>
        {
            e.ToTable("FactVentas", "Fact");
            e.HasKey(e => e.VentaKey);
            e.Property(e => e.VentaKey).ValueGeneratedOnAdd();
            e.Property(e => e.FacturaId).HasMaxLength(50);
            e.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
            e.Property(e => e.Total).HasColumnType("decimal(18,2)");
            e.Property(e => e.Status).HasMaxLength(20);

            e.HasOne(e => e.DimTiempo).WithMany().HasForeignKey(e => e.TiempoKey).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(e => e.DimProducto).WithMany().HasForeignKey(e => e.ProductoKey).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(e => e.DimCliente).WithMany().HasForeignKey(e => e.ClienteKey).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(e => e.DimVendedor).WithMany().HasForeignKey(e => e.VendedorKey).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(e => e.DimFuente).WithMany().HasForeignKey(e => e.FuenteKey).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
