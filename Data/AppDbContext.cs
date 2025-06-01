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
        public required DbSet<Product> Product { get; set; }
        public required DbSet<Inventory> Inventory { get; set; }
        public required DbSet<Catalogue> Catalogue { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.SKU).HasMaxLength(50);
                entity.HasIndex(e => e.SKU).IsUnique();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            });

            // Inventory configuration
            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.HasKey(e => e.InventoryId);
                entity.HasOne(d => d.Product)
                    .WithOne(p => p.Inventory)
                    .HasForeignKey<Inventory>(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Manual data seeding for testing
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, Name = "iPhone 15 Pro", SKU = "IPH15PRO001", Category = "Smartphones", Brand = "Apple", Price = 1499.99m, Description = "Latest iPhone with Pro features", IsActive = true },
                new Product { ProductId = 2, Name = "Samsung Galaxy S24", SKU = "SGS24001", Category = "Smartphones", Brand = "Samsung", Price = 1299.99m, Description = "Premium Android smartphone", IsActive = true },
                new Product { ProductId = 3, Name = "MacBook Pro 14", SKU = "MBP14001", Category = "Laptops", Brand = "Apple", Price = 2999.99m, Description = "Professional laptop for creative work", IsActive = true },
                new Product { ProductId = 4, Name = "Dell XPS 13", SKU = "DXPS13001", Category = "Laptops", Brand = "Dell", Price = 1399.99m, Description = "Compact and powerful ultrabook", IsActive = true },
                new Product { ProductId = 5, Name = "iPad Pro 12.9", SKU = "IPD129001", Category = "Tablets", Brand = "Apple", Price = 1099.99m, Description = "Powerful tablet with M2 chip", IsActive = true },
                new Product { ProductId = 6, Name = "Google Pixel 8", SKU = "GPX8001", Category = "Smartphones", Brand = "Google", Price = 999.99m, Description = "Google's flagship smartphone", IsActive = true },
                new Product { ProductId = 7, Name = "Surface Laptop 5", SKU = "MSLP5001", Category = "Laptops", Brand = "Microsoft", Price = 1299.99m, Description = "Sleek design with Windows 11", IsActive = true },
                new Product { ProductId = 8, Name = "Lenovo ThinkPad X1 Carbon", SKU = "TPX1C001", Category = "Laptops", Brand = "Lenovo", Price = 1599.99m, Description = "Business-class ultrabook", IsActive = true }
            );


            modelBuilder.Entity<Inventory>().HasData(
                new Inventory { InventoryId = 1, ProductId = 1, StockLevel = 25, LowStockThreshold = 5 },
                new Inventory { InventoryId = 2, ProductId = 2, StockLevel = 30, LowStockThreshold = 10 },
                new Inventory { InventoryId = 3, ProductId = 3, StockLevel = 8, LowStockThreshold = 5 },
                new Inventory { InventoryId = 4, ProductId = 4, StockLevel = 12, LowStockThreshold = 3 },
                new Inventory { InventoryId = 5, ProductId = 5, StockLevel = 20, LowStockThreshold = 5 },
                new Inventory { InventoryId = 6, ProductId = 6, StockLevel = 15, LowStockThreshold = 4 },
                new Inventory { InventoryId = 7, ProductId = 7, StockLevel = 10, LowStockThreshold = 2 },
                new Inventory { InventoryId = 8, ProductId = 8, StockLevel = 18, LowStockThreshold = 6 }
            );
        }
    }
}
