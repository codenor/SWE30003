using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStoreAss3.Models.Product
{
    public class Product
    {
        [Key] 
        public int ProductId { get; set; }

        [Required] 
        [StringLength(50)] 
        public required string SKU { get; set; }

        [Required] 
        [StringLength(100)] 
        public required string Name { get; set; }

        [Required] 
        [StringLength(500)] 
        public required string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required] 
        [StringLength(50)] 
        public required string Category { get; set; }
        
        public string? Brand { get; set; }
        
        public string? ImagePath { get; set; }
        
        public string? Specifications { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime LastModified { get; set; } = DateTime.Now;
        
        // Navigation property for inventory
        public virtual Inventory? Inventory { get; set; }
        
        // Foreign key for catalogue
        public int? CatalogueId { get; set; }
        public virtual Catalogue? Catalogue { get; set; }

    }
}