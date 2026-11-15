using Microsoft.EntityFrameworkCore;
using OutdoorShop.Models;

namespace OutdoorShop.Data
{
    public class ApplicationDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CustomerProfile> CustomerProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Name).HasMaxLength(150).IsRequired();
                entity.Property(p => p.Description).HasMaxLength(2000);
                entity.Property(p => p.ImageUrl).HasMaxLength(500);
                entity.Property(p => p.Price).HasPrecision(18, 2);
                entity.HasOne(p => p.Category).WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
                entity.Property(c => c.Description).HasMaxLength(500);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.CustomerName).HasMaxLength(150).IsRequired();
                entity.Property(o => o.Email).HasMaxLength(150).IsRequired();
                entity.Property(o => o.Phone).HasMaxLength(30).IsRequired();
                entity.Property(o => o.Address).HasMaxLength(300).IsRequired();
                entity.Property(o => o.TotalPrice).HasPrecision(18, 2);
                entity.Property(o => o.Status).HasMaxLength(30).IsRequired().HasDefaultValue(OrderStatuses.Pending);
                entity.Property(o => o.CheckoutKey).HasMaxLength(36);
                entity.HasIndex(o => o.CheckoutKey).IsUnique();
                entity.HasIndex(o => o.CustomerUserId);
                entity.HasOne<Microsoft.AspNetCore.Identity.IdentityUser>()
                    .WithMany().HasForeignKey(o => o.CustomerUserId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<CustomerProfile>(entity =>
            {
                entity.HasKey(profile => profile.UserId);
                entity.Property(profile => profile.UserId).HasMaxLength(255);
                entity.Property(profile => profile.FullName).HasMaxLength(150).IsRequired();
                entity.Property(profile => profile.Phone).HasMaxLength(30);
                entity.Property(profile => profile.Address).HasMaxLength(300);
                entity.HasOne<Microsoft.AspNetCore.Identity.IdentityUser>()
                    .WithOne().HasForeignKey<CustomerProfile>(profile => profile.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(i => i.ProductName).HasMaxLength(150).IsRequired();
                entity.Property(i => i.Price).HasPrecision(18, 2);
                entity.Property(i => i.TotalPrice).HasPrecision(18, 2);
                entity.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
