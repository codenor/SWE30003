using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStoreAss3.Models
{
    public class ShoppingCartItem
    {
        [Key]
        public int ShoppingCartItemId { get; set; }
        
        [Required]
        public int ShoppingCartId { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        public DateTime AddedDate { get; set; } = DateTime.Now;

        public virtual ShoppingCart? ShoppingCart { get; set; }
        public virtual Product? Product { get; set; }
        
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}