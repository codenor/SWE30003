using Microsoft.EntityFrameworkCore;
using ElectronicsStoreAss3.Models;

namespace ElectronicsStoreAss3.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public required DbSet<Test> Test { get; set; }
        public required DbSet<Product> Product { get; set; }
        public required DbSet<Inventory> Inventory { get; set; }
        public required DbSet<Catalogue> Catalogue { get; set; }
    }
}
