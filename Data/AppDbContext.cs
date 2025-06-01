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
        }
    }
}
