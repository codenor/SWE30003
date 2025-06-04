using ElectronicsStoreAss3.Models;
using ElectronicsStoreAss3.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ElectronicsStoreAss3.Helper;
using ElectronicsStoreAss3.Models.ShoppingCart;
using ElectronicsStoreAss3.Models.Statistics;

namespace ElectronicsStoreAss3.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly IStatisticsService _statisticsService;
        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(
            IShoppingCartService shoppingCartService,
            IProductService productService,
            IStatisticsService statisticsService,
            ILogger<ShoppingCartController> logger)
        {
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _statisticsService = statisticsService;
            _logger = logger;
        }

        // GET: /ShoppingCart
        public async Task<IActionResult> Index()
        {
            try
            {
                int? accountId = GetAccountId();
                var cart = accountId.HasValue
                    ? await _shoppingCartService.GetCartByAccountIdAsync(accountId.Value)
                    : await _shoppingCartService.GetCartBySessionIdAsync(Session.GetOrCreate(HttpContext));

                // Get recommendations if cart is not empty
                if (!cart.IsEmpty)
                {
                    await EnrichCartWithRecommendations(cart);
                }
                else
                {
                    // For empty cart, get top selling products
                    await EnrichCartWithPopularProducts(cart);
                }

                return View(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shopping cart");

                // Create a minimal cart to prevent errors
                var fallbackCart = new ShoppingCartViewModel
                {
                    SessionId = Session.GetOrCreate(HttpContext)
                };

                // Add error message
                ViewBag.StatisticsError = "Unable to load product statistics. Please try refreshing the page.";

                return View(fallbackCart);
            }
        }

        // Helper method to add recommendations to cart
        private async Task EnrichCartWithRecommendations(ShoppingCartViewModel cart)
        {
            try
            {
                // Get product categories from cart
                var categories = cart.CartItems
                    .Select(i => i.ProductSKU.Split('-')[0])
                    .Distinct()
                    .ToList();

                // Get product IDs from cart
                var productIds = cart.CartItems
                    .Select(i => i.ProductId)
                    .ToList();

                // Get recommended products based on cart contents
                var recommendedProducts = await _productService.GetRelatedProductsAsync(productIds, categories, 3);

                // Add recommendations to ViewBag
                ViewBag.RecommendedProducts = recommendedProducts;

                // Get frequently bought together products
                var fromDate = DateTime.Now.AddMonths(-3);
                var topProducts = await _statisticsService.GetTopProductsAsync(fromDate, DateTime.Now, 5);

                // Filter out products already in cart
                var complementaryProducts = topProducts
                    .Where(p => !productIds.Contains(p.ProductId))
                    .Take(3)
                    .ToList();

                ViewBag.ComplementaryProducts = complementaryProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recommendations");
                // Set empty lists to prevent null reference exceptions in the view
                ViewBag.RecommendedProducts = new List<ProductPerformance>();
                ViewBag.ComplementaryProducts = new List<ProductPerformance>();
                ViewBag.StatisticsError = "Unable to load product recommendations.";
            }
        }

        // Helper method to add popular products for empty cart
        private async Task EnrichCartWithPopularProducts(ShoppingCartViewModel cart)
        {
            try
            {
                var fromDate = DateTime.Now.AddMonths(-1);
                var popularProducts = await _statisticsService.GetTopProductsAsync(fromDate, DateTime.Now, 4);
                ViewBag.PopularProducts = popularProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading popular products");
                // Set empty list to prevent null reference exceptions
                ViewBag.PopularProducts = new List<ProductPerformance>();
                ViewBag.StatisticsError = "Unable to load popular products.";
            }
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
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Quantity must be at least 1." });
                }

                TempData["ErrorMessage"] = "Quantity must be at least 1.";
                return RedirectToAction("Index");
            }

            bool success = await _shoppingCartService.UpdateCartItemQuantityAsync(cartItemId, quantity);

            if (success)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Quantity updated successfully!" });
                }

                TempData["SuccessMessage"] = "Quantity updated successfully!";
                TempData["ToastMessage"] = "Quantity updated!";
                TempData["ToastType"] = "success";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                        { success = false, message = "Failed to update quantity. Please check stock availability." });
                }

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
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Item removed from cart successfully!" });
                }

                TempData["SuccessMessage"] = "Item removed from cart successfully!";
                TempData["ToastMessage"] = "Item removed from cart!";
                TempData["ToastType"] = "success";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to remove item from cart." });
                }

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
            bool success;
            int? accountId = GetAccountId();

            if (accountId.HasValue)
            {
                success = await _shoppingCartService.ClearCartAsync(accountId.Value);
            }
            else
            {
                success = await _shoppingCartService.ClearCartAsync(Session.GetOrCreate(HttpContext));
            }

            if (success)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Your cart has been cleared successfully!" });
                }

                TempData["SuccessMessage"] = "Your cart has been cleared successfully!";
                TempData["ToastMessage"] = "Cart cleared!";
                TempData["ToastType"] = "success";
            }
            else
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Failed to clear your cart." });
                }

                TempData["ErrorMessage"] = "Failed to clear your cart.";
                TempData["ToastMessage"] = "Failed to clear cart!";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction("Index");
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