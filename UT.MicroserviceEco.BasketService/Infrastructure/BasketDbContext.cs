using Microsoft.EntityFrameworkCore;

using UT.MicroserviceEco.BasketService.Domain;

namespace UT.MicroserviceEco.BasketService.Infrastructure;

internal sealed class BasketDbContext(DbContextOptions<BasketDbContext> options) : DbContext(options)
{
    public DbSet<BasketItem> BasketItems => Set<BasketItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BasketItem>(entity =>
        {
            entity.ToTable("basket_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ProductId).IsRequired();
            entity.Property(x => x.Quantity).IsRequired();
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2).IsRequired();
            entity.Property(x => x.AddedAtUtc).IsRequired();
            entity.HasIndex(x => x.UserName);
        });
    }
}
