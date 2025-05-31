using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStoreAss3.Models
{
    public class Product
    {
        [Key] 
        public int ProductId { get; set; }

        [Required] 
        [StringLength(50)] 
        public string SKU { get; set; }

        [Required] 
        [StringLength(100)] 
        public string Name { get; set; }

        [Required] 
        [StringLength(500)] 
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required] 
        [StringLength(50)] 
        public string Category { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
    }
}