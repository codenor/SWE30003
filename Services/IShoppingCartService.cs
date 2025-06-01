using ElectronicsStoreAss3.Models;

namespace ElectronicsStoreAss3.Services
{
    public interface IShoppingCartService
    {
        Task<ShoppingCartViewModel> GetCartBySessionIdAsync(string sessionId);
        Task<ShoppingCartViewModel> GetCartByAccountIdAsync(int accountId);
        Task<bool> AddToCartAsync(string sessionId, AddToCartRequest request);
        Task<bool> AddToCartAsync(int accountId, AddToCartRequest request);
        Task<bool> UpdateCartItemQuantityAsync(int cartItemId, int newQuantity);
        Task<bool> RemoveFromCartAsync(int cartItemId);
        Task<bool> ClearCartAsync(string sessionId);
        Task<bool> ClearCartAsync(int accountId);
        Task<bool> MergeCartsAsync(string sessionId, int accountId);
        Task<int> GetCartItemCountAsync(string sessionId);
        Task<int> GetCartItemCountAsync(int accountId);
        Task<bool> ValidateCartItemsAsync(string sessionId);
        Task<bool> ValidateCartItemsAsync(int accountId);
        Task<ShoppingCartViewModel> CreateCartAsync(string sessionId, int? accountId = null);
    }
}