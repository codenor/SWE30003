using Microsoft.AspNetCore.Mvc;
using ElectronicsStoreAss3.Data;
using ElectronicsStoreAss3.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStoreAss3.Models
{
    public class HeroCarouselViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public HeroCarouselViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 3)
        {
            // Get featured products for hero carousel - using OrderByDescending on ProductId
            // to get the newest products instead of random selection
            var featuredProducts = await _context.Product
                .Where(p => p.IsActive)
                .Where(p => !string.IsNullOrEmpty(p.ImagePath))
                .OrderByDescending(p => p.ProductId) // Get newest products
                .Take(count)
                .ToListAsync();
            
            return View(featuredProducts);
        }
    }
} 