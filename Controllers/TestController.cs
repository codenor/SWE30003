using Microsoft.AspNetCore.Mvc;
using ElectronicsStoreAss3.Data;

namespace ElectronicsStoreAss3.Controllers
{
    public class TestController : Controller
    {
        private readonly AppDbContext _context;

        public TestController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var entries = _context.Test.ToList();
            return View(entries);
        }

        [HttpPost]
        public IActionResult Add(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _context.Test.Add(new ElectronicsStoreAss3.Models.Test { Message = message });
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}