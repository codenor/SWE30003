using Microsoft.AspNetCore.Mvc;
using ElectronicsStoreAss3.Data;
using ElectronicsStoreAss3.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStoreAss3.Models
{
    public class FeaturedProductsViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public FeaturedProductsViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 4)
        {
            var products = await _context.Product
                .Where(p => p.IsActive)
                .ToListAsync();

            // Perform random ordering in memory after data is retrieved from database
            products = products
                .OrderBy(p => Guid.NewGuid())
                .Take(count)
                .ToList();

            return View(products);
        }
    }
}