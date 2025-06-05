using ElectronicsStoreAss3.Data;
using ElectronicsStoreAss3.Models.Product;
using ElectronicsStoreAss3.Models.Statistics;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStoreAss3.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        
        public ProductService(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<ProductViewModel>> GetAllProductsAsync()
        {
            var products = await _context.Product 
                .Include(p => p.Inventory)
                .Where(p => p.IsActive)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImagePath = p.ImagePath,
                    SKU = p.SKU,
                    Category = p.Category,
                    Brand = p.Brand,
                    Specifications = p.Specifications,
                    IsActive = p.IsActive,
                    StockLevel = p.Inventory != null ? p.Inventory.StockLevel : 0,
                    LowStockThreshold = p.Inventory != null ? p.Inventory.LowStockThreshold : 10
                })
                .ToListAsync();
                
            return products;
        }
        
        public async Task<ProductViewModel?> GetProductByIdAsync(int id)
        {
            var product = await _context.Product  // Using singular form
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);
                
            if (product == null) return null;
            
            return new ProductViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImagePath = product.ImagePath,
                SKU = product.SKU,
                Category = product.Category,
                Brand = product.Brand,
                Specifications = product.Specifications,
                IsActive = product.IsActive,
                StockLevel = product.Inventory?.StockLevel ?? 0,
                LowStockThreshold = product.Inventory?.LowStockThreshold ?? 10
            };
        }
        
        public async Task<ProductViewModel?> GetProductBySkuAsync(string sku)
        {
            var product = await _context.Product  // Using singular form
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.SKU == sku && p.IsActive);
                
            if (product == null) return null;
            
            return new ProductViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImagePath = product.ImagePath,
                SKU = product.SKU,
                Category = product.Category,
                Brand = product.Brand,
                Specifications = product.Specifications,
                IsActive = product.IsActive,
                StockLevel = product.Inventory?.StockLevel ?? 0,
                LowStockThreshold = product.Inventory?.LowStockThreshold ?? 10
            };
        }
        
        public async Task<ProductSearchViewModel> SearchProductsAsync(ProductSearchViewModel searchModel)
        {
            var query = _context.Product  
                .Include(p => p.Inventory)
                .Where(p => p.IsActive);
            
            // Apply search filters
            if (!string.IsNullOrEmpty(searchModel.SearchTerm))
            {
                var searchTerm = searchModel.SearchTerm.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) || 
                    p.Description.ToLower().Contains(searchTerm) ||
                    p.SKU.ToLower().Contains(searchTerm) ||
                    p.Brand.ToLower().Contains(searchTerm));
            }
            
            if (!string.IsNullOrEmpty(searchModel.Category))
            {
                query = query.Where(p => p.Category == searchModel.Category);
            }
            
            if (!string.IsNullOrEmpty(searchModel.Brand))
            {
                query = query.Where(p => p.Brand == searchModel.Brand);
            }
            
            if (searchModel.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= searchModel.MinPrice.Value);
            }
            
            if (searchModel.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= searchModel.MaxPrice.Value);
            }
            
            // Get total count
            searchModel.TotalProducts = await query.CountAsync();
            
            // Get all filtered data first, then apply sorting on client side
            var allFilteredProducts = await query
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImagePath = p.ImagePath,
                    SKU = p.SKU,
                    Category = p.Category,
                    Brand = p.Brand,
                    Specifications = p.Specifications,
                    IsActive = p.IsActive,
                    StockLevel = p.Inventory != null ? p.Inventory.StockLevel : 0,
                    LowStockThreshold = p.Inventory != null ? p.Inventory.LowStockThreshold : 10
                })
                .ToListAsync();
            
            // Apply sorting on the client side (in memory)
            var sortedProducts = searchModel.SortBy.ToLower() switch
            {
                "price" => searchModel.SortOrder.ToUpper() == "DESC" 
                    ? allFilteredProducts.OrderByDescending(p => p.Price).ToList()
                    : allFilteredProducts.OrderBy(p => p.Price).ToList(),
                "category" => searchModel.SortOrder.ToUpper() == "DESC"
                    ? allFilteredProducts.OrderByDescending(p => p.Category).ToList()
                    : allFilteredProducts.OrderBy(p => p.Category).ToList(),
                "brand" => searchModel.SortOrder.ToUpper() == "DESC"
                    ? allFilteredProducts.OrderByDescending(p => p.Brand).ToList()
                    : allFilteredProducts.OrderBy(p => p.Brand).ToList(),
                _ => searchModel.SortOrder.ToUpper() == "DESC"
                    ? allFilteredProducts.OrderByDescending(p => p.Name).ToList()
                    : allFilteredProducts.OrderBy(p => p.Name).ToList()
            };
            
            // Apply pagination on the sorted results
            var paginatedProducts = sortedProducts
                .Skip((searchModel.Page - 1) * searchModel.PageSize)
                .Take(searchModel.PageSize)
                .ToList();
            
            searchModel.Products = paginatedProducts;
            searchModel.Categories = (await GetCategoriesAsync()).ToList();
            searchModel.Brands = (await GetBrandsAsync()).ToList();
            
            return searchModel;
        }
        
        public async Task<bool> CreateProductAsync(ProductViewModel productViewModel)
        {
            try
            {
                // Check if SKU is unique
                if (!await IsSkuUniqueAsync(productViewModel.SKU))
                {
                    return false;
                }
                
                var product = new Product
                {
                    Name = productViewModel.Name,
                    Description = productViewModel.Description,
                    Price = productViewModel.Price,
                    ImagePath = productViewModel.ImagePath,
                    SKU = productViewModel.SKU,
                    Category = productViewModel.Category,
                    Brand = productViewModel.Brand,
                    Specifications = productViewModel.Specifications,
                    IsActive = productViewModel.IsActive,
                    CreatedDate = DateTime.Now
                };
                
                _context.Product.Add(product); 
                await _context.SaveChangesAsync();
                
                // Create inventory record
                var inventory = new Inventory
                {
                    ProductId = product.ProductId,
                    StockLevel = productViewModel.StockLevel,
                    LowStockThreshold = productViewModel.LowStockThreshold,
                    LastUpdated = DateTime.Now
                };
                
                _context.Inventory.Add(inventory);  
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<bool> UpdateProductAsync(ProductViewModel productViewModel)
        {
            try
            {
                var product = await _context.Product 
                    .Include(p => p.Inventory)
                    .FirstOrDefaultAsync(p => p.ProductId == productViewModel.ProductId);
                
                if (product == null) return false;
                
                // Check if SKU is unique (excluding current product)
                if (!await IsSkuUniqueAsync(productViewModel.SKU, productViewModel.ProductId))
                {
                    return false;
                }
                
                product.Name = productViewModel.Name;
                product.Description = productViewModel.Description;
                product.Price = productViewModel.Price;
                product.ImagePath = productViewModel.ImagePath;
                product.SKU = productViewModel.SKU;
                product.Category = productViewModel.Category;
                product.Brand = productViewModel.Brand;
                product.Specifications = productViewModel.Specifications;
                product.IsActive = productViewModel.IsActive;
                product.LastModified = DateTime.Now;
                
                // Update inventory
                if (product.Inventory != null)
                {
                    product.Inventory.StockLevel = productViewModel.StockLevel;
                    product.Inventory.LowStockThreshold = productViewModel.LowStockThreshold;
                    product.Inventory.LastUpdated = DateTime.Now;
                }
                else
                {
                    var inventory = new Inventory
                    {
                        ProductId = product.ProductId,
                        StockLevel = productViewModel.StockLevel,
                        LowStockThreshold = productViewModel.LowStockThreshold,
                        LastUpdated = DateTime.Now
                    };
                    _context.Inventory.Add(inventory);  
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Product.FindAsync(id);  
                if (product == null) return false;
                
                // Soft delete - just mark as inactive
                product.IsActive = false;
                product.LastModified = DateTime.Now;
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<bool> UpdateStockLevelAsync(int productId, int newStockLevel)
        {
            try
            {
                var inventory = await _context.Inventory  // Using singular form
                    .FirstOrDefaultAsync(i => i.ProductId == productId);
                
                if (inventory == null) return false;
                
                inventory.StockLevel = newStockLevel;
                inventory.LastUpdated = DateTime.Now;
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public async Task<IEnumerable<ProductViewModel>> GetLowStockProductsAsync()
        {
            var products = await _context.Product 
                .Include(p => p.Inventory)
                .Where(p => p.IsActive && p.Inventory != null && 
                           p.Inventory.StockLevel <= p.Inventory.LowStockThreshold)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImagePath = p.ImagePath,
                    SKU = p.SKU,
                    Category = p.Category,
                    Brand = p.Brand,
                    StockLevel = p.Inventory.StockLevel,
                    LowStockThreshold = p.Inventory.LowStockThreshold
                })
                .ToListAsync();
                
            return products;
        }
        
        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _context.Product  // Using singular form
                .Where(p => p.IsActive)
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<string>> GetBrandsAsync()
        {
            return await _context.Product  
                .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Brand))
                .Select(p => p.Brand)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();
        }
        
        public async Task<bool> IsSkuUniqueAsync(string sku, int? excludeProductId = null)
        {
            var query = _context.Product.Where(p => p.SKU == sku);  
            
            if (excludeProductId.HasValue)
            {
                query = query.Where(p => p.ProductId != excludeProductId.Value);
            }
            
            return !await query.AnyAsync();
        }
        
        public async Task<List<ProductPerformance>> GetRelatedProductsAsync(List<int> productIds, List<string> categories, int count = 3)
        {
            try
            {
                // Find products that are in the same categories but not already in the cart
                var relatedProducts = await _context.Product
                    .Include(p => p.Inventory)
                    .Where(p => p.IsActive && 
                               !productIds.Contains(p.ProductId) && 
                               categories.Contains(p.Category))
                    .Select(p => new ProductPerformance
                    {
                        ProductId = p.ProductId,
                        ProductName = p.Name,
                        SKU = p.SKU,
                        Category = p.Category,
                        AveragePrice = p.Price,
                        Revenue = 0, // Not applicable for recommendations
                        QuantitySold = p.Inventory != null ? p.Inventory.StockLevel : 0,
                        OrderCount = 0 // Not applicable for recommendations
                    })
                    .Take(count)
                    .ToListAsync();

                return relatedProducts;
            }
            catch (Exception)
            {
                // Return empty list in case of error
                return new List<ProductPerformance>();
            }
        }
    }
}