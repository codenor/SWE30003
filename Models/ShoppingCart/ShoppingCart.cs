using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStoreAss3.Models
{
    public class ShoppingCart
    {
        [Key]
        public int ShoppingCartId { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string SessionId { get; set; }
        
        [ForeignKey("Account")] 
        public int? AccountId { get; set; } 

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
        
        // Customer
        public virtual Customer? Customer { get; set; }
        public virtual ICollection<ShoppingCartItem> CartItems { get; set; } = new List<ShoppingCartItem>();
        
        public decimal TotalAmount => CartItems.Sum(item => item.TotalPrice);
        public int TotalItems => CartItems.Sum(item => item.Quantity);
        
    }
}