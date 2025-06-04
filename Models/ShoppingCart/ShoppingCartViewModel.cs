using System.ComponentModel.DataAnnotations;
using ElectronicsStoreAss3.Models.Statistics;

namespace ElectronicsStoreAss3.Models
{
    public class ShoppingCartViewModel
    {
        public int ShoppingCartId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public int? AccountId { get; set; }
        public List<ShoppingCartItemViewModel> CartItems { get; set; } = new List<ShoppingCartItemViewModel>();
        
        // Financial calculations
        public decimal TotalAmount => CartItems.Sum(item => item.TotalPrice);
        public int TotalItems => CartItems.Sum(item => item.Quantity);
        public bool IsEmpty => !CartItems.Any();
        
        public decimal SubTotal => TotalAmount;
        public decimal GST => TotalAmount * 0.10m; // 10% GST for Australia
        public decimal ShippingFee { get; set; } = 0m;
        public decimal Total => SubTotal + ShippingFee + GST;
        public decimal GrandTotal => Total; // Alias for Total to match controller expectations
    }
    
    public class ShoppingCartItemViewModel
    {
        public int ShoppingCartItemId { get; set; }
        public int ShoppingCartId { get; set; }
        public int ProductId { get; set; }

        [Required]
        [Range(1, 999, ErrorMessage = "Quantity must be between 1 and 999")]
        public int Quantity { get; set; }
        
        public decimal UnitPrice { get; set; }
        public DateTime AddedDate { get; set; }
        
        // Product information
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public string? ProductImagePath { get; set; }
        public int ProductStockLevel { get; set; }
        
        // Calculated properties
        public decimal TotalPrice => Quantity * UnitPrice;
        public bool IsInStock => ProductStockLevel > 0;
        public bool IsQuantityAvailable => Quantity <= ProductStockLevel;
    }

    public class AddToCartRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 999, ErrorMessage = "Quantity must be between 1 and 999")]
        public int Quantity { get; set; } = 1; 
    }

    public class UpdateCartItemRequest
    {
        [Required] 
        public int CartItemId { get; set; }
        
        [Required]
        [Range(1, 999, ErrorMessage = "Quantity must be between 1 and 999")]
        public int Quantity { get; set; }
    }
}