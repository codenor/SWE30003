using System.ComponentModel.DataAnnotations;

namespace ElectronicsStoreAss3.Models
{
    public class Catalogue
    {
        [Key]
        public int CatalogueId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Collection of products in this catalogue
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}