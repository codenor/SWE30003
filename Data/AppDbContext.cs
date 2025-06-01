using Microsoft.EntityFrameworkCore;
using ElectronicsStoreAss3.Models;

namespace ElectronicsStoreAss3.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Test 
        public required DbSet<Test> Test { get; set; }

        // Accounts
        public required DbSet<Account> Accounts { get; set; }
        public required DbSet<Customer> Customers { get; set; }
        public DbSet<Owner> Owners { get; set; }


        // Products
         public DbSet<Product> Product { get; set; } = null!; 
        public DbSet<Inventory> Inventory { get; set; } = null!;
        public DbSet<Catalogue> Catalogues { get; set; } = null!;
        public DbSet<ShoppingCart> ShoppingCarts { get; set; } = null!;
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.SKU).IsUnique();

                // Product to Inventory (One-to-One)
                entity.HasOne(p => p.Inventory)
                    .WithOne(i => i.Product)
                    .HasForeignKey<Inventory>(i => i.ProductId);

                // Product to Catalogue (Many-to-One)
                entity.HasOne(p => p.Catalogue)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CatalogueId);
            });

            // Inventory configuration
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.HasKey(e => e.InventoryId);

                entity.HasOne(i => i.Product)
                    .WithOne(p => p.Inventory)
                    .HasForeignKey<Inventory>(i => i.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Catalogue configuration
            modelBuilder.Entity<Catalogue>(entity => { entity.HasKey(e => e.CatalogueId); });

            // ShoppingCart configuration
            modelBuilder.Entity<ShoppingCart>(entity =>
            {
                entity.HasKey(e => e.ShoppingCartId);
                entity.Property(e => e.SessionId).HasMaxLength(100);
                entity.HasIndex(e => e.SessionId);
            });

            // ShoppingCartItem configuration
            modelBuilder.Entity<ShoppingCartItem>(entity =>
            {
                entity.HasKey(e => e.ShoppingCartItemId);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(d => d.ShoppingCart)
                    .WithMany(p => p.CartItems)
                    .HasForeignKey(d => d.ShoppingCartId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Product)
                    .WithMany()
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
