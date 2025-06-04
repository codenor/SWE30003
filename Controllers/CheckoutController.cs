using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElectronicsStoreAss3.Data;
using ElectronicsStoreAss3.Models;
using ElectronicsStoreAss3.Services;
using System.Security.Claims;
using ElectronicsStoreAss3.Helper;
using ElectronicsStoreAss3.Models.Account;
using ElectronicsStoreAss3.Models.Checkout;
using ElectronicsStoreAss3.Models.Order;
using ElectronicsStoreAss3.Models.Shipment;
using ElectronicsStoreAss3.Models.ShoppingCart;

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
                    TempData["ToastMessage"] = "Your cart is empty. Please add items before checkout.";
                    TempData["ToastType"] = "warning";
                    return RedirectToAction("Index", "ShoppingCart");
                }

                // Validate cart items are still in stock
                if (!await ValidateCartStockAsync(cart))
                {
                    TempData["ToastMessage"] =
                        "Some items in your cart are no longer available. Please review your cart.";
                    TempData["ToastType"] = "warning";
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
            // Always reload the cart from the backend - don't trust form data
            model.Cart = await GetCurrentCartAsync() ?? new ShoppingCartViewModel();

            // Validate that cart has items
            if (model.Cart.IsEmpty)
            {
                TempData["ToastMessage"] = "Your cart is empty. Please add items before checkout.";
                TempData["ToastType"] = "warning";
                return RedirectToAction("Index", "ShoppingCart");
            }

            // Custom validation for Australian mobile format
            if (!string.IsNullOrEmpty(model.Mobile))
            {
                var mobileRegex = new System.Text.RegularExpressions.Regex(@"^(?:\+614|04)\d{8}$");
                if (!mobileRegex.IsMatch(model.Mobile))
                {
                    ModelState.AddModelError("Mobile",
                        "Enter a valid Australian mobile number (04XX XXX XXX or +614X XXX XXX)");
                }
            }

            // Remove Cart from ModelState validation since we reload it from backend
            ModelState.Remove("Cart");

            if (!ModelState.IsValid)
            {
                // Cart is already loaded above
                return View("Details", model);
            }

            try
            {
                _logger.LogInformation("Starting order processing for {Email}", model.Email);

                var result = await ProcessOrderAsync(model);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Order {OrderId} processed successfully", result.OrderId);

                    TempData["OrderId"] = result.OrderId;
                    TempData["TrackingNumber"] = result.TrackingNumber;
                    TempData["CustomerName"] = $"{model.FirstName} {model.LastName}";
                    TempData["OrderTotal"] = model.Cart.GrandTotal.ToString("F2");

                    return RedirectToAction("Success");
                }
                else
                {
                    _logger.LogWarning("Order processing failed: {Error}", result.ErrorMessage);
                    TempData["ToastMessage"] = result.ErrorMessage;
                    TempData["ToastType"] = "error";
                    model.Cart = await GetCurrentCartAsync() ?? new ShoppingCartViewModel();
                    return View("Details", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during order processing for {Email}", model.Email);
                TempData["ToastMessage"] = "An unexpected error occurred during checkout. Please try again.";
                TempData["ToastType"] = "error";
                model.Cart = await GetCurrentCartAsync() ?? new ShoppingCartViewModel();
                return View("Details", model);
            }
        }

        // GET: /Checkout/Success
        [HttpGet]
        public IActionResult Success()
        {
            var orderId = TempData["OrderId"] as int?;
            var trackingNumber = TempData["TrackingNumber"] as string;
            var customerName = TempData["CustomerName"] as string;
            var orderTotal = TempData["OrderTotal"] as string;

            if (!orderId.HasValue)
            {
                // No order data, redirect to home
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new CheckoutSuccessViewModel
            {
                OrderId = orderId.Value,
                TrackingNumber = trackingNumber,
                CustomerName = customerName,
                OrderTotal = orderTotal
            };

            return View(viewModel);
        }


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

            return id;
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

        private async Task<bool> ValidateCartStockAsync(ShoppingCartViewModel cart)
        {
            foreach (var item in cart.CartItems)
            {
                var inventory = await _context.Inventory
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                if (inventory == null || inventory.StockLevel < item.Quantity)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<OrderProcessingResult> ProcessOrderAsync(CheckoutViewModel model)
        {
            var cart = await GetCurrentCartAsync();
            if (cart == null || !cart.CartItems.Any())
            {
                return OrderProcessingResult.Failure("Cart is empty or could not be retrieved");
            }

            // Final stock validation
            var stockValidation = await ValidateStockLevelsAsync(cart.CartItems);
            if (!stockValidation.IsValid)
            {
                return OrderProcessingResult.Failure(stockValidation.ErrorMessage);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Creating order for {Email}", model.Email);

                // Update customer address if logged in
                var accountId = GetCurrentAccountId();
                if (accountId.HasValue && !string.IsNullOrEmpty(model.Address))
                {
                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.AccountId == accountId.Value);

                    if (customer != null)
                    {
                        customer.Address = model.Address;
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Updated shipping address for customer {CustomerId}", customer.Id);
                    }
                }

                // 1. Create order
                var order = await CreateOrderAsync(model, cart);
                _logger.LogInformation("Order {OrderId} created", order.OrderId);

                // 2. Create order items
                await CreateOrderItemsAsync(order.OrderId, cart.CartItems);
                _logger.LogInformation("Order items created for order {OrderId}", order.OrderId);

                // 3. Update inventory
                await UpdateInventoryAsync(cart.CartItems);
                _logger.LogInformation("Inventory updated for order {OrderId}", order.OrderId);

                // 4. Create shipment
                var shipment = await CreateShipmentAsync(order.OrderId, model);
                _logger.LogInformation("Shipment {TrackingNumber} created for order {OrderId}", shipment.TrackingNumber,
                    order.OrderId);

                // 5. Generate invoice
                var invoice = await _invoiceService.GenerateInvoiceAsync(order.OrderId);
                _logger.LogInformation("Invoice {InvoiceNumber} generated for order {OrderId}", invoice.InvoiceNumber,
                    order.OrderId);

                // 6. Send invoice email (demo - just log it)
                await _invoiceService.SendInvoiceEmailAsync(invoice.InvoiceId);
                _logger.LogInformation("Invoice email sent for order {OrderId}", order.OrderId);

                // 7. Clear cart
                await ClearCurrentCartAsync();
                _logger.LogInformation("Cart cleared for order {OrderId}", order.OrderId);

                await transaction.CommitAsync();

                _logger.LogInformation("Order {OrderId} completed successfully with tracking {TrackingNumber}",
                    order.OrderId, shipment.TrackingNumber);

                return OrderProcessingResult.Success(order.OrderId, shipment.TrackingNumber);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing order for {Email}", model.Email);
                throw;
            }
        }

        private async Task<(bool IsValid, string ErrorMessage)> ValidateStockLevelsAsync(
            IEnumerable<ShoppingCartItemViewModel> cartItems)
        {
            foreach (var item in cartItems)
            {
                var inventory = await _context.Inventory
                    .Include(i => i.Product)
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId);

                if (inventory == null)
                {
                    return (false, $"Product '{item.ProductName}' is no longer available");
                }

                if (inventory.StockLevel < item.Quantity)
                {
                    return (false,
                        $"Insufficient stock for '{item.ProductName}'. Only {inventory.StockLevel} available, but {item.Quantity} requested");
                }

                if (!inventory.Product.IsActive)
                {
                    return (false, $"Product '{item.ProductName}' is no longer available for purchase");
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
                OrderNotes =
                    $"Order placed by {model.FirstName} {model.LastName} ({model.Email}). Mobile: {model.Mobile}",
                LastModified = DateTime.Now
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

                    // Ensure stock doesn't go negative
                    if (inventory.StockLevel < 0)
                    {
                        _logger.LogWarning("Stock level went negative for product {ProductId}. Setting to 0.",
                            item.ProductId);
                        inventory.StockLevel = 0;
                    }
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
                EstimatedDeliveryDate = DateTime.Now.AddBusinessDays(5), // Use business days
                ShippingAddress = shippingAddress,
                CreatedDate = DateTime.Now,
                LastUpdated = DateTime.Now,
                CarrierName = "AWE Express",
                TrackingNumber = $"AWE{DateTime.Now:yyyyMMdd}{orderId:D6}",
                DeliveryNotes = "Standard delivery"
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
    }

    // Helper class for order processing results
    public class OrderProcessingResult
    {
        public bool IsSuccess { get; private set; }
        public int OrderId { get; private set; }
        public string? TrackingNumber { get; private set; }
        public string? ErrorMessage { get; private set; }

        private OrderProcessingResult()
        {
        }

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

    // Extension method for business days calculation
    public static class DateTimeExtensions
    {
        public static DateTime AddBusinessDays(this DateTime startDate, int businessDays)
        {
            if (businessDays == 0) return startDate;

            int direction = Math.Sign(businessDays);
            int businessDaysRemaining = Math.Abs(businessDays);
            DateTime current = startDate;

            while (businessDaysRemaining > 0)
            {
                current = current.AddDays(direction);

                // Skip weekends
                if (current.DayOfWeek != DayOfWeek.Saturday &&
                    current.DayOfWeek != DayOfWeek.Sunday)
                {
                    businessDaysRemaining--;
                }
            }

            return current;
        }
    }
}