using ElectronicsStoreAss3.Models;
using ElectronicsStoreAss3.Services;
using ElectronicsStoreAss3.Helpers;
using Microsoft.AspNetCore.Mvc;
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

        // POST: /ShoppingCart/AddToCart - Traditional Form Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string returnUrl = "")
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid product or quantity.";
                return RedirectToReturnUrl(returnUrl);
            }

            var request = new AddToCartRequest { ProductId = productId, Quantity = quantity };
            
            int? accountId = GetAccountId();
            bool success = accountId.HasValue
                ? await _shoppingCartService.AddToCartAsync(accountId.Value, request)
                : await _shoppingCartService.AddToCartAsync(Session.GetOrCreate(HttpContext), request);

            if (success)
            {
                // Get product name for better message
                var product = await _productService.GetProductByIdAsync(productId);
                var productName = product?.Name ?? "Product";
                
                TempData["SuccessMessage"] = $"{productName} (x{quantity}) added to cart successfully!";
                TempData["ToastMessage"] = $"{productName} added to cart!";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to add product to cart. Please check stock availability.";
                TempData["ToastMessage"] = "Failed to add product to cart!";
                TempData["ToastType"] = "error";
            }

            return RedirectToReturnUrl(returnUrl);
        }

        // POST: /ShoppingCart/UpdateQuantity - Traditional Form Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            if (quantity < 1)
            {
                TempData["ErrorMessage"] = "Quantity must be at least 1.";
                return RedirectToAction("Index");
            }

            bool success = await _shoppingCartService.UpdateCartItemQuantityAsync(cartItemId, quantity);

            if (success)
            {
                TempData["SuccessMessage"] = "Quantity updated successfully!";
                TempData["ToastMessage"] = "Quantity updated!";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update quantity. Please check stock availability.";
                TempData["ToastMessage"] = "Failed to update quantity!";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction("Index");
        }

        // POST: /ShoppingCart/RemoveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var success = await _shoppingCartService.RemoveFromCartAsync(cartItemId);

            if (success)
            {
                TempData["SuccessMessage"] = "Item removed from cart successfully!";
                TempData["ToastMessage"] = "Item removed from cart!";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to remove item from cart.";
                TempData["ToastMessage"] = "Failed to remove item!";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction("Index");
        }

        // POST: /ShoppingCart/ClearCart 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MergeCartOnLogin()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var accountId = GetAccountId();
                var sessionId = HttpContext.Session.GetString("CartSessionId");
        
                if (accountId.HasValue && !string.IsNullOrEmpty(sessionId))
                {
                    var mergeSuccess = await _shoppingCartService.MergeCartsAsync(sessionId, accountId.Value);
            
                    if (mergeSuccess)
                    {
                        // Clear the session cart ID since it's now merged
                        HttpContext.Session.Remove("CartSessionId");
                
                        TempData["SuccessMessage"] = "Your cart has been updated with your account!";
                        TempData["ToastMessage"] = "Cart merged successfully!";
                        TempData["ToastType"] = "success";
                    }
                }
            }
    
            return RedirectToAction("Index");
        }

     

        // GET: Cart count for navigation (used by _CartCount partial)
        public async Task<int> GetCartCountAsync()
        {
            try
            {
                int? accountId = GetAccountId();
                return accountId.HasValue
                    ? await _shoppingCartService.GetCartItemCountAsync(accountId.Value)
                    : await _shoppingCartService.GetCartItemCountAsync(Session.GetOrCreate(HttpContext));
            }
            catch
            {
                return 0;
            }
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

        // Helper method to handle return URLs
        private IActionResult RedirectToReturnUrl(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            // Default redirect locations in priority order
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && referer.Contains(Request.Host.Value))
            {
                return Redirect(referer);
            }
            
            // Fallback to product catalogue
            return RedirectToAction("Catalogue", "Product");
        }
    }
}