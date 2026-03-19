using CQRS.POC.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CQRS.POC.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(p => p.Description)
                      .HasMaxLength(1000);

                entity.Property(p => p.Price)
                      .IsRequired();

                entity.Property(p => p.Stock)
                      .IsRequired();

                entity.Property(p => p.IsActive)
                      .IsRequired();

                entity.Property(p => p.CreatedAt)
                      .IsRequired();

                entity.Property(p => p.UpdatedAt)
                      .IsRequired();
            });
        }
    }
}
