using ElectronicsStoreAss3.Data;
using ElectronicsStoreAss3.Models.Order;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStoreAss3.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(AppDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Include(o => o.Customer)
                    .Include(o => o.Shipment)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all orders");
                return new List<Order>();
            }
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Include(o => o.Customer)
                    .Include(o => o.Shipment)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting order with ID {OrderId}", orderId);
                return null;
            }
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Include(o => o.Shipment)
                    .Where(o => o.AccountId == userId)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting orders for user with ID {UserId}", userId);
                return new List<Order>();
            }
        }

        public async Task<List<Order>> GetRecentOrdersAsync(int count)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Include(o => o.Customer)
                    .Include(o => o.Shipment)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting recent orders");
                return new List<Order>();
            }
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            try
            {
                // Set created date
                order.OrderDate = DateTime.Now;
                order.LastModified = DateTime.Now;

                // Calculate the total if not already set
                if (order.TotalAmount <= 0)
                {
                    order.TotalAmount = order.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
                }

                // Add order to database
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new order with ID {OrderId}", order.OrderId);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order");
                return null;
            }
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            try
            {
                order.LastModified = DateTime.Now;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated order with ID {OrderId}", order.OrderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating order with ID {OrderId}", order.OrderId);
                return false;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found for status update", orderId);
                    return false;
                }

                order.Status = status;
                order.LastModified = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated status to {Status} for order with ID {OrderId}", status, orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating status for order with ID {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Shipment)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found for cancellation", orderId);
                    return false;
                }

                if (!order.CanBeCancelled())
                {
                    _logger.LogWarning("Order with ID {OrderId} cannot be cancelled in its current state", orderId);
                    return false;
                }

                // Update order status
                order.Status = "Cancelled";
                order.LastModified = DateTime.Now;
                order.OrderNotes = (order.OrderNotes ?? "") + $"\nCancelled on {DateTime.Now}";

                // Update shipment status if it exists
                if (order.Shipment != null)
                {
                    order.Shipment.Status = "Cancelled";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Cancelled order with ID {OrderId}", orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cancelling order with ID {OrderId}", orderId);
                return false;
            }
        }


        public async Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Orders.AsQueryable();

                if (startDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(o => o.OrderDate <= endDate.Value);
                }

                // Only include completed or shipped orders
                query = query.Where(o => o.Status == "Completed" || o.Status == "Shipped");

                return await query.SumAsync(o => o.TotalAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calculating total sales");
                return 0;
            }
        }

        public async Task<int> GetOrderCountAsync(string status = null)
        {
            try
            {
                var query = _context.Orders.AsQueryable();

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(o => o.Status == status);
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting order count");
                return 0;
            }
        }


        public async Task<bool> AddOrderItemAsync(int orderId, OrderItem item)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found for adding item", orderId);
                    return false;
                }

                // Check if the item already exists in the order
                var existingItem = order.OrderItems
                    .FirstOrDefault(oi => oi.ProductId == item.ProductId);

                if (existingItem != null)
                {
                    // Update quantity if item already exists
                    existingItem.Quantity += item.Quantity;
                }
                else
                {
                    // Add new item to order
                    item.OrderId = orderId;
                    order.OrderItems.Add(item);
                }

                // Update order total
                order.TotalAmount = order.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
                order.LastModified = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Added item to order with ID {OrderId}", orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding item to order with ID {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> RemoveOrderItemAsync(int orderId, int orderItemId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found for removing item", orderId);
                    return false;
                }

                var item = order.OrderItems.FirstOrDefault(oi => oi.Id == orderItemId);
                if (item == null)
                {
                    _logger.LogWarning("Order item with ID {OrderItemId} not found in order {OrderId}", orderItemId,
                        orderId);
                    return false;
                }

                // Remove item from order
                order.OrderItems.Remove(item);

                // Update order total
                order.TotalAmount = order.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
                order.LastModified = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed item {OrderItemId} from order with ID {OrderId}", orderItemId, orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing item from order with ID {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> UpdateOrderItemQuantityAsync(int orderId, int orderItemId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    _logger.LogWarning("Cannot set quantity to {Quantity} for item {OrderItemId}", quantity,
                        orderItemId);
                    return false;
                }

                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found for updating item quantity", orderId);
                    return false;
                }

                var item = order.OrderItems.FirstOrDefault(oi => oi.Id == orderItemId);
                if (item == null)
                {
                    _logger.LogWarning("Order item with ID {OrderItemId} not found in order {OrderId}", orderItemId,
                        orderId);
                    return false;
                }

                // Update item quantity
                item.Quantity = quantity;

                // Update order total
                order.TotalAmount = order.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
                order.LastModified = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated quantity to {Quantity} for item {OrderItemId} in order {OrderId}",
                    quantity, orderItemId, orderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating item quantity in order with ID {OrderId}", orderId);
                return false;
            }
        }
    }
}