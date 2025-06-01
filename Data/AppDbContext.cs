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
        // TODO: Add owner later
        // public DbSet<Owner> Owners { get; set; }
        
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
            
            // Seed data for Products
            modelBuilder.Entity<Product>().HasData(
                new Product 
                { 
                    ProductId = 1, 
                    SKU = "IPH15", 
                    Name = "iPhone 15", 
                    Description = "Latest iPhone", 
                    Price = 999.99m, 
                    Category = "Smartphones",
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    LastModified = DateTime.Now
                },
                new Product 
                { 
                    ProductId = 2, 
                    SKU = "SGS24", 
                    Name = "Samsung Galaxy S24", 
                    Description = "Android flagship", 
                    Price = 899.99m, 
                    Category = "Smartphones",
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    LastModified = DateTime.Now
                },
                new Product 
                { 
                    ProductId = 3, 
                    SKU = "MBP14", 
                    Name = "MacBook Pro", 
                    Description = "Professional laptop", 
                    Price = 1999.99m, 
                    Category = "Laptops",
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    LastModified = DateTime.Now
                }
            );
            
            // Seed data for Inventories
            modelBuilder.Entity<Inventory>().HasData(
                new Inventory 
                { 
                    InventoryId = 1, 
                    ProductId = 1, 
                    StockLevel = 10, 
                    LowStockThreshold = 5,
                    LastUpdated = DateTime.Now
                },
                new Inventory 
                { 
                    InventoryId = 2, 
                    ProductId = 2, 
                    StockLevel = 15, 
                    LowStockThreshold = 5,
                    LastUpdated = DateTime.Now
                },
                new Inventory 
                { 
                    InventoryId = 3, 
                    ProductId = 3, 
                    StockLevel = 5, 
                    LowStockThreshold = 2,
                    LastUpdated = DateTime.Now
                }
            );
            
            // Seed data for Catalogue
            modelBuilder.Entity<Catalogue>().HasData(
                new Catalogue
                {
                    CatalogueId = 1,
                    Name = "Electronics",
                    Description = "All electronic products",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                }
            );
        }
    }
}