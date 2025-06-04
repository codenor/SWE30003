using Microsoft.EntityFrameworkCore;
using ElectronicsStoreAss3.Models; 
using ElectronicsStoreAss3.Models.ShoppingCart;

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
        public DbSet<Product> Product { get; set; } 
        public DbSet<Inventory> Inventory { get; set; } 
        public DbSet<Catalogue> Catalogues { get; set; } 
        public DbSet<ShoppingCart> ShoppingCarts { get; set; } 
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; } 

        // Orders and Shipments
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Shipment> Shipments { get; set; }

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
                    .HasForeignKey<Inventory>(i => i.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Product to Catalogue (Many-to-One)
                entity.HasOne(p => p.Catalogue)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CatalogueId)
                    .OnDelete(DeleteBehavior.SetNull);
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
            modelBuilder.Entity<Catalogue>(entity => 
            { 
                entity.HasKey(e => e.CatalogueId); 
            });

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

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");

                // Order to Account (Many-to-One) - ALLOW NULL for guest orders
                entity.HasOne(o => o.Account)
                    .WithMany()
                    .HasForeignKey(o => o.AccountId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false); // This is crucial - allows NULL AccountId

                // Order to Customer (Many-to-One) - ALLOW NULL for guest orders  
                entity.HasOne(o => o.Customer)
                    .WithMany()
                    .HasForeignKey(o => o.AccountId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false); // This is crucial - allows NULL AccountId
            });

            // OrderItem configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Product)
                    .WithMany()
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Shipment configuration
            modelBuilder.Entity<Shipment>(entity =>
            {
                entity.HasKey(e => e.ShipmentId);
                
                // One-to-One relationship: Order to Shipment
                entity.HasOne(s => s.Order)
                    .WithOne(o => o.Shipment)
                    .HasForeignKey<Shipment>(s => s.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.TrackingNumber).IsUnique();
                entity.Property(e => e.Status).HasDefaultValue("Processing");
            });

            // Account configuration
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Customer configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(c => c.Account)
                    .WithOne()
                    .HasForeignKey<Customer>(c => c.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Owner configuration
            modelBuilder.Entity<Owner>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(o => o.Account)
                    .WithOne()
                    .HasForeignKey<Owner>(o => o.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}