using System.ComponentModel.DataAnnotations;

namespace ElectronicsStoreAss3.Models.Product
{
    public class ProductViewModel
    {
        /*
         *  ProductViewModel - Purpose: Product Data formatting for views/forms
         *  
         */
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public string? ImagePath { get; set; }

        [Required(ErrorMessage = "SKU is required")]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; } = string.Empty;

        public string? Brand { get; set; }

        public string? Specifications { get; set; }

        public bool IsActive { get; set; } = true;

        // Inventory information
        public int StockLevel { get; set; }

        public int LowStockThreshold { get; set; } = 10;

        public bool IsInStock => StockLevel > 0;

        public bool IsLowStock => StockLevel <= LowStockThreshold;
    }

    /*
     *  ProductSearchViewModel - Purpose: Simple implementation for handling search and filtering
     * 
     */
    public class ProductSearchViewModel
    {
        public string? SearchTerm { get; set; }

        public string? Category { get; set; }

        public string? Brand { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public string SortBy { get; set; } = "Name";

        public string SortOrder { get; set; } = "ASC";

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 12;

        public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();

        public int TotalProducts { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalProducts / PageSize);

        public List<string> Categories { get; set; } = new List<string>();

        public List<string> Brands { get; set; } = new List<string>();
    }
}