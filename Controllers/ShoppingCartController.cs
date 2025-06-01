using ElectronicsStoreAss3.Models;
using ElectronicsStoreAss3.Services;
using Microsoft.AspNetCore.Mvc;

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
            var sessionId = GetOrCreateSessionId();
            var cart = await _shoppingCartService.GetCartBySessionIdAsync(sessionId);
            return View(cart);
        }
        
        // POST: /ShoppingCart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string returnUrl = "")
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid product or quantity.";
                return RedirectToReturnUrl(returnUrl);
            }

            var sessionId = GetOrCreateSessionId();
            var request = new AddToCartRequest { ProductId = productId, Quantity = quantity };
            var success = await _shoppingCartService.AddToCartAsync(sessionId, request);
            
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
        
        // POST: /ShoppingCart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            if (quantity < 1)
            {
                TempData["ErrorMessage"] = "Quantity must be at least 1.";
                return RedirectToAction("Index");
            }

            var success = await _shoppingCartService.UpdateCartItemQuantityAsync(cartItemId, quantity);
            
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
        public async Task<IActionResult> ClearCart()
        {
            var sessionId = GetOrCreateSessionId();
            var success = await _shoppingCartService.ClearCartAsync(sessionId);
            
            if (success)
            {
                TempData["SuccessMessage"] = "Cart cleared successfully!";
                TempData["ToastMessage"] = "Cart cleared!";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to clear cart.";
                TempData["ToastMessage"] = "Failed to clear cart!";
                TempData["ToastType"] = "error";
            }
            
            return RedirectToAction("Index");
        }
        
        // GET: Cart count for navigation (called from layout)
        public async Task<int> GetCartCountAsync()
        {
            try
            {
                var sessionId = GetOrCreateSessionId();
                return await _shoppingCartService.GetCartItemCountAsync(sessionId);
            }
            catch
            {
                return 0;
            }
        }
        
        // Helper method to get or create session ID
        private string GetOrCreateSessionId()
        {
            var sessionId = HttpContext.Session.GetString("CartSessionId");
            
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("CartSessionId", sessionId);
            }
            
            return sessionId;
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