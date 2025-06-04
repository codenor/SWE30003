using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStoreAss3.Models.Product
{
    public class Inventory
    {
        [Key]
        public int InventoryId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public int StockLevel { get; set; }
        
        public int LowStockThreshold { get; set; } = 10;

        public DateTime LastUpdated { get; set; } = DateTime.Now;
        
        // Navigation property
        public virtual Product? Product { get; set; }
        
        public bool IsLowStock => StockLevel <= LowStockThreshold;
        public bool IsInStock => StockLevel > 0;
    }
}