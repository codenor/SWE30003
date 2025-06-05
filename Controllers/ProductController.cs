using ElectronicsStoreAss3.Models;
using ElectronicsStoreAss3.Models.Product;
using ElectronicsStoreAss3.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ElectronicsStoreAss3.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: Product 
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Index(string? searchTerm = null, string? searchCategory = null)
        {
            var allProducts = await _productService.GetAllProductsAsync();

            // Apply filters if search terms are provided
            if (!string.IsNullOrWhiteSpace(searchTerm) || !string.IsNullOrWhiteSpace(searchCategory))
            {
                var filteredProducts = allProducts.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    filteredProducts = filteredProducts.Where(p =>
                        p.Name.ToLower().Contains(searchTerm) ||
                        p.SKU.ToLower().Contains(searchTerm) ||
                        p.Description.ToLower().Contains(searchTerm) ||
                        p.Brand.ToLower().Contains(searchTerm));
                }

                if (!string.IsNullOrWhiteSpace(searchCategory))
                {
                    filteredProducts = filteredProducts.Where(p =>
                        p.Category.Equals(searchCategory, StringComparison.OrdinalIgnoreCase));
                }

                ViewBag.SearchTerm = searchTerm;
                ViewBag.SearchCategory = searchCategory;
                ViewBag.Categories = allProducts.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();

                return View(filteredProducts.ToList());
            }

            ViewBag.Categories = allProducts.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
            return View(allProducts);
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
        [Authorize(Roles = "Owner")]
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
        [Authorize(Roles = "Owner")]
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
        [Authorize(Roles = "Owner")]
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
        [Authorize(Roles = "Owner")]
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
        [Authorize(Roles = "Owner")]
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
        [Authorize(Roles = "Owner")]
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

            //
            query = query.ToLower();

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

            query = query.ToLower();
            
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
            return Json(new
            {
                products,
                totalCount = results.TotalProducts,
                hasMore = results.TotalProducts > searchModel.PageSize
            });
        }
    }
}