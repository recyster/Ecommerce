using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Order> Orders => Set<Order>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(e =>
            {
                e.HasKey(o => o.Id);
                e.Property(o => o.UserId).IsRequired();
                e.Property(o => o.ProductId).IsRequired();
                e.Property(o => o.Quantity).IsRequired();
                e.Property(o => o.PaymentMethod).IsRequired();
                e.Property(o => o.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
