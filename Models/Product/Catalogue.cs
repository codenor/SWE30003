using System.ComponentModel.DataAnnotations;

namespace ElectronicsStoreAss3.Models
{
    // Catalogue - Purpose: Groups products into categories
    public class Catalogue
    {
        [Key]
        public int CatalogueId { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }
        
        [StringLength(500)]
        public required string Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Collection of products in this catalogue
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}