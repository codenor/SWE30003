using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElectronicsStoreAss3.Data;
using ElectronicsStoreAss3.Models;
using ElectronicsStoreAss3.Services;
using ElectronicsStoreAss3.Helpers;
using System.Security.Claims;

namespace ElectronicsStoreAss3.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IShipmentService _shipmentService;
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            AppDbContext context, 
            IShoppingCartService shoppingCartService,
            IShipmentService shipmentService,
            IInvoiceService invoiceService,
            ILogger<CheckoutController> logger)
        {
            _context = context;
            _shoppingCartService = shoppingCartService;
            _shipmentService = shipmentService;
            _invoiceService = invoiceService;
            _logger = logger;
        }

        // GET: /Checkout/Details
        [HttpGet]
        public async Task<IActionResult> Details()
        {
            try
            {
                var cart = await GetCurrentCartAsync();
                if (cart == null || !cart.CartItems.Any())
                {
                    TempData["ToastMessage"] = "Your cart is empty.";
                    TempData["ToastType"] = "error";
                    return RedirectToAction("Index", "ShoppingCart");
                }

                var customer = await GetCurrentCustomerAsync();
                var viewModel = CreateCheckoutViewModel(cart, customer);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading checkout page");
                TempData["ToastMessage"] = "Error loading checkout. Please try again.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "ShoppingCart");
            }
        }

        // POST: /Checkout/ProcessPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload cart data for redisplay
                model.Cart = await GetCurrentCartAsync() ?? new ShoppingCartViewModel();
                return View("Details", model);
            }

            try
            {
                var result = await ProcessOrderAsync(model);
                
                if (result.IsSuccess)
                {
                    TempData["ToastMessage"] = $"Order #{result.OrderId} placed successfully! Tracking: {result.TrackingNumber}";
                    TempData["ToastType"] = "success";
                    return RedirectToAction("Details", "Order", new { id = result.OrderId });
                }
                else
                {
                    TempData["ToastMessage"] = result.ErrorMessage;
                    TempData["ToastType"] = "error";
                    model.Cart = await GetCurrentCartAsync() ?? new ShoppingCartViewModel();
                    return View("Details", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during order processing");
                TempData["ToastMessage"] = "An unexpected error occurred. Please try again.";
                TempData["ToastType"] = "error";
                model.Cart = await GetCurrentCartAsync() ?? new ShoppingCartViewModel();
                return View("Details", model);
            }
        }

        #region Private Helper Methods

        private async Task<ShoppingCartViewModel?> GetCurrentCartAsync()
        {
            var accountId = GetCurrentAccountId();
            
            return accountId.HasValue
                ? await _shoppingCartService.GetCartByAccountIdAsync(accountId.Value)
                : await _shoppingCartService.GetCartBySessionIdAsync(Session.GetOrCreate(HttpContext));
        }

        private async Task<Customer?> GetCurrentCustomerAsync()
        {
            var accountId = GetCurrentAccountId();
            
            if (!accountId.HasValue) return null;

            return await _context.Customers
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.AccountId == accountId.Value);
        }

        private int? GetCurrentAccountId()
        {
            if (!User.Identity?.IsAuthenticated == true) return null;
            
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idStr, out int id)) return null;

            // Validate account exists (you might want to cache this)
            var accountExists = _context.Accounts.Any(a => a.Id == id);
            return accountExists ? id : null;
        }

        private static CheckoutViewModel CreateCheckoutViewModel(ShoppingCartViewModel cart, Customer? customer)
        {
            return new CheckoutViewModel
            {
                Cart = cart,
                FirstName = customer?.FirstName ?? "",
                LastName = customer?.LastName ?? "",
                Mobile = customer?.Mobile ?? "",
                Email = customer?.Email ?? customer?.Account?.Email ?? "",
                Address = customer?.Address ?? ""
            };
        }

        private async Task<OrderProcessingResult> ProcessOrderAsync(CheckoutViewModel model)
        {
            var cart = await GetCurrentCartAsync();
            if (cart == null || !cart.CartItems.Any())
            {
                return OrderProcessingResult.Failure("Cart is empty");
            }

            // Validate stock levels
            var stockValidation = await ValidateStockLevelsAsync(cart.CartItems);
            if (!stockValidation.IsValid)
            {
                return OrderProcessingResult.Failure(stockValidation.ErrorMessage);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // 1. Create order
                var order = await CreateOrderAsync(model, cart);
                
                // 2. Create order items
                await CreateOrderItemsAsync(order.OrderId, cart.CartItems);
                
                // 3. Update inventory
                await UpdateInventoryAsync(cart.CartItems);
                
                // 4. Create shipment
                var shipment = await CreateShipmentAsync(order.OrderId, model);
                
                // 5. Generate invoice
                var invoice = await _invoiceService.GenerateInvoiceAsync(order.OrderId);
                
                // 6. Send invoice email
                await _invoiceService.SendInvoiceEmailAsync(invoice.InvoiceId);
                
                // 7. Clear cart
                await ClearCurrentCartAsync();
                
                await transaction.CommitAsync();
                
                _logger.LogInformation("Order {OrderId} completed successfully", order.OrderId);
                
                return OrderProcessingResult.Success(order.OrderId, shipment.TrackingNumber);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing order");
                throw;
            }
        }

        private async Task<(bool IsValid, string ErrorMessage)> ValidateStockLevelsAsync(IEnumerable<ShoppingCartItemViewModel> cartItems)
        {
            foreach (var item in cartItems)
            {
                var inventory = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);
                
                if (inventory == null || inventory.StockLevel < item.Quantity)
                {
                    return (false, $"Insufficient stock for {item.ProductName}");
                }
            }
            
            return (true, "");
        }

        private async Task<Order> CreateOrderAsync(CheckoutViewModel model, ShoppingCartViewModel cart)
        {
            var order = new Order
            {
                AccountId = GetCurrentAccountId(),
                OrderDate = DateTime.Now,
                TotalAmount = cart.GrandTotal,
                Status = "Confirmed",
                OrderNotes = $"Order placed by {model.FirstName} {model.LastName} ({model.Email})"
            };
            
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            
            return order;
        }

        private async Task CreateOrderItemsAsync(int orderId, IEnumerable<ShoppingCartItemViewModel> cartItems)
        {
            var orderItems = cartItems.Select(item => new OrderItem
            {
                OrderId = orderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
            
            _context.OrderItems.AddRange(orderItems);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateInventoryAsync(IEnumerable<ShoppingCartItemViewModel> cartItems)
        {
            foreach (var item in cartItems)
            {
                var inventory = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);
                
                if (inventory != null)
                {
                    inventory.StockLevel -= item.Quantity;
                    inventory.LastUpdated = DateTime.Now;
                }
            }
            
            await _context.SaveChangesAsync();
        }

        private async Task<Shipment> CreateShipmentAsync(int orderId, CheckoutViewModel model)
        {
            var shippingAddress = $"{model.FirstName} {model.LastName}\n{model.Address}";
            var shipment = new Shipment
            {
                OrderId = orderId,
                Status = "Processing",
                EstimatedDeliveryDate = DateTime.Now.AddDays(5),
                ShippingAddress = shippingAddress,
                CreatedDate = DateTime.Now,
                LastUpdated = DateTime.Now,
                CarrierName = "AWE Express",
                TrackingNumber = $"AWE{DateTime.Now:yyyyMMdd}{orderId:D6}"
            };
            
            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();
            
            return shipment;
        }

        private async Task ClearCurrentCartAsync()
        {
            var accountId = GetCurrentAccountId();
            
            if (accountId.HasValue)
                await _shoppingCartService.ClearCartAsync(accountId.Value);
            else
                await _shoppingCartService.ClearCartAsync(Session.GetOrCreate(HttpContext));
        }

        #endregion
    }

    // Helper class for order processing results
    public class OrderProcessingResult
    {
        public bool IsSuccess { get; private set; }
        public int OrderId { get; private set; }
        public string? TrackingNumber { get; private set; }
        public string? ErrorMessage { get; private set; }

        private OrderProcessingResult() { }

        public static OrderProcessingResult Success(int orderId, string? trackingNumber)
        {
            return new OrderProcessingResult
            {
                IsSuccess = true,
                OrderId = orderId,
                TrackingNumber = trackingNumber
            };
        }

        public static OrderProcessingResult Failure(string errorMessage)
        {
            return new OrderProcessingResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
