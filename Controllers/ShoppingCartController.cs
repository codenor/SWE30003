using ElectronicsStoreAss3.Models;
using ElectronicsStoreAss3.Services;
using Microsoft.AspNetCore.Mvc;
using ElectronicsStoreAss3.Helpers;
using System.Security.Claims;

namespace ElectronicsStoreAss3.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;

        public ShoppingCartController(IShoppingCartService shoppingCartService, IProductService productService)
        {
            _shoppingCartService = shoppingCartService;
            _productService = productService;
        }

        // GET: /ShoppingCart
        public async Task<IActionResult> Index()
        {
            int? accountId = GetAccountId();
            var cart = accountId.HasValue
                ? await _shoppingCartService.GetCartByAccountIdAsync(accountId.Value)
                : await _shoppingCartService.GetCartBySessionIdAsync(Session.GetOrCreate(HttpContext));

            return View(cart);
        }

        // POST: /ShoppingCart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request" });

            int? accountId = GetAccountId();
            bool success = accountId.HasValue
                ? await _shoppingCartService.AddToCartAsync(accountId.Value, request)
                : await _shoppingCartService.AddToCartAsync(Session.GetOrCreate(HttpContext), request);

            int itemCount = accountId.HasValue
                ? await _shoppingCartService.GetCartItemCountAsync(accountId.Value)
                : await _shoppingCartService.GetCartItemCountAsync(Session.GetOrCreate(HttpContext));

            if (success)
                return Json(new { success = true, message = "Product added to cart", itemCount });

            return Json(new { success = false, message = "Failed to add product to cart. Please check stock availability." });
        }

        // POST: /ShoppingCart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartItemRequest request)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request" });

            bool success = await _shoppingCartService.UpdateCartItemQuantityAsync(request.CartItemId, request.Quantity);

            if (success)
            {
                int? accountId = GetAccountId();
                var cart = accountId.HasValue
                    ? await _shoppingCartService.GetCartByAccountIdAsync(accountId.Value)
                    : await _shoppingCartService.GetCartBySessionIdAsync(Session.GetOrCreate(HttpContext));

                return Json(new
                {
                    success = true,
                    message = "Quantity updated",
                    totalAmount = cart.TotalAmount,
                    totalItems = cart.TotalItems,
                    gst = cart.GST,
                    grandTotal = cart.GrandTotal
                });
            }

            return Json(new { success = false, message = "Failed to update quantity. Please check stock availability." });
        }

        // POST: /ShoppingCart/RemoveItem
        [HttpPost]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var success = await _shoppingCartService.RemoveFromCartAsync(cartItemId);

            if (success)
            {
                int? accountId = GetAccountId();
                var cart = accountId.HasValue
                    ? await _shoppingCartService.GetCartByAccountIdAsync(accountId.Value)
                    : await _shoppingCartService.GetCartBySessionIdAsync(Session.GetOrCreate(HttpContext));

                return Json(new
                {
                    success = true,
                    message = "Item removed from cart",
                    totalAmount = cart.TotalAmount,
                    totalItems = cart.TotalItems,
                    gst = cart.GST,
                    grandTotal = cart.GrandTotal
                });
            }

            return Json(new { success = false, message = "Failed to remove item from cart" });
        }

        // POST: /ShoppingCart/ClearCart
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            int? accountId = GetAccountId();
            var success = accountId.HasValue
                ? await _shoppingCartService.ClearCartAsync(accountId.Value)
                : await _shoppingCartService.ClearCartAsync(Session.GetOrCreate(HttpContext));

            if (success)
                return Json(new { success = true, message = "Cart cleared successfully" });

            return Json(new { success = false, message = "Failed to clear cart" });
        }

        // GET: /ShoppingCart/GetCartCount
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            int? accountId = GetAccountId();
            var count = accountId.HasValue
                ? await _shoppingCartService.GetCartItemCountAsync(accountId.Value)
                : await _shoppingCartService.GetCartItemCountAsync(Session.GetOrCreate(HttpContext));

            return Json(new { count });
        }

        // GET: /ShoppingCart/ValidateCart
        [HttpGet]
        public async Task<IActionResult> ValidateCart()
        {
            int? accountId = GetAccountId();
            var isValid = accountId.HasValue
                ? await _shoppingCartService.ValidateCartItemsAsync(accountId.Value)
                : await _shoppingCartService.ValidateCartItemsAsync(Session.GetOrCreate(HttpContext));

            return Json(new { isValid });
        }

        // Partial view for cart summary (can be used in layout)
        public async Task<IActionResult> CartSummary()
        {
            int? accountId = GetAccountId();
            var cart = accountId.HasValue
                ? await _shoppingCartService.GetCartByAccountIdAsync(accountId.Value)
                : await _shoppingCartService.GetCartBySessionIdAsync(Session.GetOrCreate(HttpContext));

            return PartialView("_CartSummary", cart);
        }

        // Helper to get AccountId from claims
        private int? GetAccountId()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idStr, out int id)) return id;
            }
            return null;
        }
    }
}
