using ElectronicsStoreAss3.Models;
using Microsoft.AspNetCore.Identity;

namespace ElectronicsStoreAss3.Data
{
    public static class Init
    {
        public static void SeedOwnerAccount(AppDbContext context)
        {
            if (context.Accounts.Any(a => a.Role == Role.Owner))
                return;

            var hasher = new PasswordHasher<object>();
            var account = new Account
            {
                Email = "owner@test.com",
                PasswordHash = hasher.HashPassword(null, "password"),
                Role = Role.Owner
            };

            context.Accounts.Add(account);
            context.SaveChanges();

            var owner = new Owner
            {
                AccountId = account.Id,
                Name = "Max",
                LastName = "Isghey"
            };

            context.Owners.Add(owner);
            context.SaveChanges();
        }

        public static void SeedTestProductsAndInventory(AppDbContext context)
        {
            if (context.Test.Any())
                return;

            var products = new List<Product>
            {
                new()
                {
                    ProductId = 1, Name = "iPhone 15 Pro", SKU = "IPH15PRO001", Category = "Smartphones",
                    Brand = "Apple", Price = 1499.99m, Description = "Latest iPhone with Pro features", IsActive = true
                },
                new()
                {
                    ProductId = 2, Name = "Samsung Galaxy S24", SKU = "SGS24001", Category = "Smartphones",
                    Brand = "Samsung", Price = 1299.99m, Description = "Premium Android smartphone", IsActive = true
                },
                new()
                {
                    ProductId = 3, Name = "MacBook Pro 14", SKU = "MBP14001", Category = "Laptops", Brand = "Apple",
                    Price = 2999.99m, Description = "Professional laptop for creative work", IsActive = true
                },
                new()
                {
                    ProductId = 4, Name = "Dell XPS 13", SKU = "DXPS13001", Category = "Laptops", Brand = "Dell",
                    Price = 1399.99m, Description = "Compact and powerful ultrabook", IsActive = true
                },
                new()
                {
                    ProductId = 5, Name = "iPad Pro 12.9", SKU = "IPD129001", Category = "Tablets", Brand = "Apple",
                    Price = 1099.99m, Description = "Powerful tablet with M2 chip", IsActive = true
                },
                new()
                {
                    ProductId = 6, Name = "Google Pixel 8", SKU = "GPX8001", Category = "Smartphones", Brand = "Google",
                    Price = 999.99m, Description = "Google's flagship smartphone", IsActive = true
                },
                new()
                {
                    ProductId = 7, Name = "Surface Laptop 5", SKU = "MSLP5001", Category = "Laptops",
                    Brand = "Microsoft", Price = 1299.99m, Description = "Sleek design with Windows 11", IsActive = true
                },
                new()
                {
                    ProductId = 8, Name = "Lenovo ThinkPad X1 Carbon", SKU = "TPX1C001", Category = "Laptops",
                    Brand = "Lenovo", Price = 1599.99m, Description = "Business-class ultrabook", IsActive = true
                }
            };

            context.Product.AddRange(products);
            context.SaveChanges();

            var inventory = new List<Inventory>
            {
                new() { InventoryId = 1, ProductId = 1, StockLevel = 25, LowStockThreshold = 5 },
                new() { InventoryId = 2, ProductId = 2, StockLevel = 30, LowStockThreshold = 10 },
                new() { InventoryId = 3, ProductId = 3, StockLevel = 8, LowStockThreshold = 5 },
                new() { InventoryId = 4, ProductId = 4, StockLevel = 12, LowStockThreshold = 3 },
                new() { InventoryId = 5, ProductId = 5, StockLevel = 20, LowStockThreshold = 5 },
                new() { InventoryId = 6, ProductId = 6, StockLevel = 15, LowStockThreshold = 4 },
                new() { InventoryId = 7, ProductId = 7, StockLevel = 10, LowStockThreshold = 2 },
                new() { InventoryId = 8, ProductId = 8, StockLevel = 18, LowStockThreshold = 6 }
            };

            context.Inventory.AddRange(inventory);
            context.SaveChanges();
        }
        
        public static void SeedAll(AppDbContext context)
        {
            SeedTestProductsAndInventory(context);
            SeedOwnerAccount(context);
        }
    }
}