using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStoreAss3.Models
{
    public class Inventory
    {
        [Key]
        public int InventoryId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public int StockLevel { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now; 
    }
}