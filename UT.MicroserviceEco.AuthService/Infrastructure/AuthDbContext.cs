using Microsoft.EntityFrameworkCore;

namespace UT.MicroserviceEco.AuthService.Infrastructure;

internal sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(400).IsRequired();
            entity.Property(x => x.PasswordSalt).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Roles).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.UserName).IsUnique();
        });
    }
}
