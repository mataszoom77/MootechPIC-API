using Microsoft.EntityFrameworkCore;
using MootechPic.API.Models;

namespace MootechPic.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<SparePart> SpareParts { get; set; }
        public DbSet<ProductSparePart> ProductSpareParts { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }

        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<SparePartImage> SparePartImages { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<RequestImage> RequestImages { get; set; }
        public DbSet<AdminResponse> AdminResponses { get; set; }
        public DbSet<AdminResponseAttachment> AdminResponseAttachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Force lowercase table names
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity?.GetTableName()?.ToLower());
            }

            // Composite key for join table
            modelBuilder.Entity<ProductSparePart>()
                .HasKey(psp => new { psp.ProductId, psp.SparePartId });

            modelBuilder.Entity<WishlistItem>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId);

            modelBuilder.Entity<SparePartImage>()
                .HasOne(spi => spi.SparePart)
                .WithMany(sp => sp.SparePartImages)
                .HasForeignKey(spi => spi.SparePartId);

            // 3) OrderItems one‐source constraint
            modelBuilder.Entity<OrderItem>()
              .HasCheckConstraint(
                "order_items_one_source",
                "(product_id IS NOT NULL)::int + (spare_part_id IS NOT NULL)::int = 1"
              );

            // 4) Order → OrderItems relationship
            modelBuilder.Entity<Order>()
              .HasMany(o => o.OrderItems)
              .WithOne(oi => oi.Order)
              .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<Request>()
                .HasMany(r => r.Images)
                .WithOne(i => i.Request)
                .HasForeignKey(i => i.RequestId);

            modelBuilder.Entity<Request>()
                .HasMany(r => r.Responses)
                .WithOne(ar => ar.Request)
                .HasForeignKey(ar => ar.RequestId);
            modelBuilder.Entity<Request>()
                .HasOne(r => r.User)
                // ← point at the new Requests collection
                .WithMany(u => u.Requests)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdminResponse>()
                .HasMany(ar => ar.Attachments)
                .WithOne(a => a.AdminResponse)
                .HasForeignKey(a => a.AdminResponseId);



        }

    }
}