using Microsoft.EntityFrameworkCore;

using UT.MicroserviceEco.ProductService.Domain;

namespace UT.MicroserviceEco.ProductService.Infrastructure;

internal sealed class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Price).HasPrecision(18, 2).IsRequired();
            entity.Property(x => x.Stock).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
        });
    }
}
