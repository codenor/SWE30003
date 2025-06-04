using ElectronicsStoreAss3.Models;
using ElectronicsStoreAss3.Models.Statistics;

namespace ElectronicsStoreAss3.Services
{
    // <summary>
    // 
    // </summary>
    public interface IProductService
    {
        Task<IEnumerable<ProductViewModel>> GetAllProductsAsync();
        Task<ProductViewModel> GetProductByIdAsync(int id);
        Task<ProductViewModel> GetProductBySkuAsync(string sku);
        Task<ProductSearchViewModel> SearchProductsAsync(ProductSearchViewModel searchModel);
        Task<bool> CreateProductAsync(ProductViewModel productViewModel);
        Task<bool> UpdateProductAsync(ProductViewModel productViewModel);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> UpdateStockLevelAsync(int productId, int newStockLevel);
        Task<IEnumerable<ProductViewModel>> GetLowStockProductsAsync();
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<IEnumerable<string>> GetBrandsAsync();
        Task<bool> IsSkuUniqueAsync(string sku, int? excludeProductId = null);
        Task<List<ProductPerformance>> GetRelatedProductsAsync(List<int> productIds, List<string> categories, int count = 3);
    }
}