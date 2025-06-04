using ElectronicsStoreAss3.Models.Order;

namespace ElectronicsStoreAss3.Services
{
    public interface IOrderService
    {
        // Order retrieval methods
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetOrdersByUserIdAsync(int userId);
        Task<List<Order>> GetRecentOrdersAsync(int count);

        // Order creation and manipulation
        Task<Order> CreateOrderAsync(Order order);
        Task<bool> UpdateOrderAsync(Order order);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<bool> CancelOrderAsync(int orderId);

        // Order statistics and analytics
        Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetOrderCountAsync(string status = null);

        // Order item management
        Task<bool> AddOrderItemAsync(int orderId, OrderItem item);
        Task<bool> RemoveOrderItemAsync(int orderId, int orderItemId);
        Task<bool> UpdateOrderItemQuantityAsync(int orderId, int orderItemId, int quantity);
    }
}