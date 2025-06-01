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
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid request" });
            }
            
            var sessionId = GetOrCreateSessionId();
            var success = await _shoppingCartService.AddToCartAsync(sessionId, request);
            
            if (success)
            {
                var itemCount = await _shoppingCartService.GetCartItemCountAsync(sessionId);
                return Json(new { success = true, message = "Product added to cart", itemCount });
            }
            
            return Json(new { success = false, message = "Failed to add product to cart. Please check stock availability." });
        }
        
        // POST: /ShoppingCart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid request" });
            }
            
            var success = await _shoppingCartService.UpdateCartItemQuantityAsync(request.CartItemId, request.Quantity);
            
            if (success)
            {
                var sessionId = GetOrCreateSessionId();
                var cart = await _shoppingCartService.GetCartBySessionIdAsync(sessionId);
                
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
                var sessionId = GetOrCreateSessionId();
                var cart = await _shoppingCartService.GetCartBySessionIdAsync(sessionId);
                
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
            var sessionId = GetOrCreateSessionId();
            var success = await _shoppingCartService.ClearCartAsync(sessionId);
            
            if (success)
            {
                return Json(new { success = true, message = "Cart cleared successfully" });
            }
            
            return Json(new { success = false, message = "Failed to clear cart" });
        }
        
        // GET: /ShoppingCart/GetCartCount
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var sessionId = GetOrCreateSessionId();
            var count = await _shoppingCartService.GetCartItemCountAsync(sessionId);
            
            return Json(new { count });
        }
        
        // GET: /ShoppingCart/ValidateCart
        [HttpGet]
        public async Task<IActionResult> ValidateCart()
        {
            var sessionId = GetOrCreateSessionId();
            var isValid = await _shoppingCartService.ValidateCartItemsAsync(sessionId);
            
            return Json(new { isValid });
        }
        
        // Partial view for cart summary (can be used in layout)
        public async Task<IActionResult> CartSummary()
        {
            var sessionId = GetOrCreateSessionId();
            var cart = await _shoppingCartService.GetCartBySessionIdAsync(sessionId);
            
            return PartialView("_CartSummary", cart);
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
    }
}