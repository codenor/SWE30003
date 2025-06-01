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

        public CheckoutController(AppDbContext context, IShoppingCartService shoppingCartService)
        {
            _context = context;
            _shoppingCartService = shoppingCartService;
        }

        // GET: /Checkout/Details
        [HttpGet]
        public async Task<IActionResult> Details()
        {
            var sessionId = Session.GetOrCreate(HttpContext);
            int? accountId = null;

            if (User.Identity?.IsAuthenticated == true)
            {
                string? idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idStr, out int parsedId))
                {
                    accountId = parsedId;
                }
            }

            var cartViewModel = accountId.HasValue
                ? await _shoppingCartService.GetCartByAccountIdAsync(accountId.Value)
                : await _shoppingCartService.GetCartBySessionIdAsync(sessionId);

            if (cartViewModel == null || !cartViewModel.CartItems.Any())
            {
                TempData["ToastMessage"] = "Your cart is empty.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var customer = accountId.HasValue
                ? await _context.Customers.Include(c => c.Account)
                    .FirstOrDefaultAsync(c => c.AccountId == accountId.Value)
                : null;

            var viewModel = new CheckoutViewModel
            {
                Cart = cartViewModel,
                FirstName = customer?.FirstName,
                LastName = customer?.LastName,
                Mobile = customer?.Mobile,
                Email = customer?.Account?.Email,
                Address = customer?.Address
            };

            return View("Details", viewModel);
        }

        // POST: /Checkout/ProcessPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(CheckoutViewModel model)
        {
            var sessionId = Session.GetOrCreate(HttpContext);
            int? accountId = null;

            if (User.Identity?.IsAuthenticated == true)
            {
                string? idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idStr, out int parsedId))
                {
                    accountId = parsedId;
                }
            }

            var cartViewModel = accountId.HasValue
                ? await _shoppingCartService.GetCartByAccountIdAsync(accountId.Value)
                : await _shoppingCartService.GetCartBySessionIdAsync(sessionId);

            if (cartViewModel == null || !cartViewModel.CartItems.Any())
            {
                TempData["ToastMessage"] = "Your cart is empty.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "ShoppingCart");
            }

            // Create order
            var order = new Order
            {
                AccountId = accountId,
                OrderDate = DateTime.Now,
                TotalAmount = cartViewModel.TotalAmount
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Add order items and update inventory
            foreach (var item in cartViewModel.CartItems)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });

                var inventory = await _context.Inventory.FirstOrDefaultAsync(i => i.ProductId == item.ProductId);
                if (inventory != null)
                {
                    inventory.StockLevel -= item.Quantity;
                    inventory.LastUpdated = DateTime.Now;
                }
            }

            // Add shipment
            _context.Shipments.Add(new Shipment
            {
                OrderId = order.Id,
                Status = "Processing",
                EstimatedDeliveryDate = DateTime.Now.AddDays(5)
            });

            // Clear cart
            if (accountId.HasValue)
                await _shoppingCartService.ClearCartAsync(accountId.Value);
            else
                await _shoppingCartService.ClearCartAsync(sessionId);

            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = $"Order #{order.Id} placed successfully!";
            TempData["ToastType"] = "success";

            return RedirectToAction("Details", "Order", new { id = order.Id });
        }
    }
}
