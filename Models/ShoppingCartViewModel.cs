using System.ComponentModel.DataAnnotations;

namespace ElectronicsStoreAss3.Models 
{
    public class ShoppingCartViewModel
    {
        public int ShoppingCartId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public DateTime LastUpdated { get; set; }
        
        public List<ShoppingCartItemViewModel> Items { get; set; } = new List<ShoppingCartItemViewModel>();
        
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public int ItemCount { get; set; }
        public bool IsEmpty => !Items.Any();
        
        // Shipping options (for future expansion)
        public decimal ShippingCost { get; set; } = 0;
        public string ShippingMethod { get; set; } = "Standard";
        
        public decimal GrandTotal => Total + ShippingCost;
    }
    
    public class ShoppingCartItemViewModel
    {
        public int ShoppingCartItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public string ProductCategory { get; set; } = string.Empty;
        public string ProductImagePath { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal CurrentPrice { get; set; } // Current product price
        public decimal LineTotal { get; set; }
        public decimal Savings { get; set; }
        public int MaxQuantity { get; set; } // Based on stock level
        public bool IsInStock { get; set; }
        public DateTime AddedDate { get; set; }
    }
    
    public class AddToCartViewModel
    {
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;
    }
}