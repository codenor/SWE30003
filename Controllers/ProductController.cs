using ElectronicsStoreAss3.Models;
using ElectronicsStoreAss3.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ElectronicsStoreAss3.Controllers
{
    [Authorize(Roles = "Owner")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: Product 
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }
        
        // GET: Product/Details/5 OR Product/Details/APPLE-IP15P-128
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id, string? sku)
        {
            ProductViewModel? product = null;

            // Try to get product by ID first
            if (id.HasValue && id.Value > 0)
            {
                product = await _productService.GetProductByIdAsync(id.Value);
            }
            // If no ID provided or not found, try by SKU
            else if (!string.IsNullOrEmpty(sku))
            {
                product = await _productService.GetProductBySkuAsync(sku);
            }

            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction("Catalogue");
            }

            // Get related products (same category, different product)
            var relatedSearchModel = new ProductSearchViewModel
            {
                Category = product.Category,
                PageSize = 4
            };
            var relatedProducts = await _productService.SearchProductsAsync(relatedSearchModel);
            ViewBag.RelatedProducts = relatedProducts.Products.Where(p => p.ProductId != product.ProductId).Take(4);

            return View(product);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            var productViewModel = new ProductViewModel
            {
                IsActive = true,
                StockLevel = 0,
                LowStockThreshold = 10
            };
            return View(productViewModel); 
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel productViewModel)
        {
            if (ModelState.IsValid)
            {
                var success = await _productService.CreateProductAsync(productViewModel);
                if (success)
                {
                    TempData["SuccessMessage"] = "Product created successfully!";
                    TempData["ToastMessage"] = "Product created!";
                    TempData["ToastType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("SKU", "A product with this SKU already exists.");
                }
            }
            return View(productViewModel);
        }

        // GET: Product/Edit/SKU
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(); 
            }
            return View(product); 
        }

        // POST: Product/Edit/SKU
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel productViewModel)
        {
            if (id != productViewModel.ProductId)
            {
                return NotFound(); 
            }

            if (ModelState.IsValid)
            {
                var success = await _productService.UpdateProductAsync(productViewModel);
                if (success)
                {
                    TempData["SuccessMessage"] = "Product updated successfully!";
                    TempData["ToastMessage"] = "Product updated!";
                    TempData["ToastType"] = "success";
                    return RedirectToAction(nameof(Index)); 
                }
                else
                {
                    ModelState.AddModelError("SKU", "A product with this SKU already exists.");
                }
            }
            return View(productViewModel); 
        }

        // GET: Product/Delete/SKU
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Product/Delete/SKU
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _productService.DeleteProductAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "Product deleted successfully!";
                TempData["ToastMessage"] = "Product deleted!";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting product.";
                TempData["ToastMessage"] = "Failed to delete product!";
                TempData["ToastType"] = "error";
            }
            return RedirectToAction(nameof(Index));
        }

 
        // GET: Product/Catalogue
        [AllowAnonymous] 
        public async Task<IActionResult> Catalogue(ProductSearchViewModel searchModel)
        {
            if (searchModel.Page <= 0) searchModel.Page = 1;
            if (searchModel.PageSize <= 0) searchModel.PageSize = 12;
            
            searchModel.PageSize = Math.Max(6, Math.Min(24, searchModel.PageSize));
            
            var result = await _productService.SearchProductsAsync(searchModel);
            
            ViewBag.HasFilters = !string.IsNullOrEmpty(searchModel.SearchTerm) || 
                                 !string.IsNullOrEmpty(searchModel.Category) || 
                                 !string.IsNullOrEmpty(searchModel.Brand) || 
                                 searchModel.MinPrice.HasValue || 
                                 searchModel.MaxPrice.HasValue;
            
            return View(result); 
        }
        
        // =========================================================================
        // AJAX SECTION - Search
        // =========================================================================
        
        // Search Suggestions - AJAX
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Json(new { suggestions = new string[0] }); 
            }

            var searchModel = new ProductSearchViewModel
            {
                SearchTerm = query,
                PageSize = 8 
            };

            var results = await _productService.SearchProductsAsync(searchModel);
            
            var suggestions = results.Products
                .Select(p => p.Name)
                .Distinct()
                .Take(6)
                .ToList();


            return Json(new { suggestions }); 
        }
        

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> QuickSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { products = new object[0] }); 
            }

            var searchModel = new ProductSearchViewModel
            {
                SearchTerm = query,
                PageSize = 5 
            };

            var results = await _productService.SearchProductsAsync(searchModel);
            
   
            var products = results.Products.Select(p => new
            {
                id = p.ProductId,
                sku = p.SKU,
                name = p.Name,
                price = p.Price.ToString("C0"), 
                image = p.ImagePath,
                category = p.Category,
                brand = p.Brand,
                inStock = p.IsInStock,
                stockLevel = p.StockLevel,
                // Provide both URL options for flexible routing
                urlById = Url.Action("Details", "Product", new { id = p.ProductId }),
                urlBySku = Url.Action("Details", "Product", new { sku = p.SKU })
            }).ToList();

            // Returns structured JSON for JavaScript consumption
            return Json(new { 
                products, 
                totalCount = results.TotalProducts,
                hasMore = results.TotalProducts > searchModel.PageSize
            });
        }

    }
}
