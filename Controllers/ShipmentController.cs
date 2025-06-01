using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElectronicsStoreAss3.Data;

namespace ElectronicsStoreAss3.Controllers
{
    public class ShipmentController : Controller
    {
        private readonly AppDbContext _context;

        public ShipmentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Shipments/
        // public IActionResult Index()
        // {
        //     var Shipments = _context.Shipments.Include(s => s.Order).ToList();
        //     return View(Shipments);
        // }

        // GET: /Shipments/Details/5
        public IActionResult Details(int id)
        {
            var Shipments = _context.Shipments
                // .Include(s => s.Order)
                .FirstOrDefault(s => s.Id == id);

            if (Shipments == null)
                return NotFound();

            return View(Shipments);
        }

        // POST: /Shipments/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int id, string newStatus)
        {
            var Shipments = _context.Shipments.Find(id);
            if (Shipments == null)
                return NotFound();

            Shipments.Status = newStatus;
            if (newStatus == "Delivered" && Shipments.DeliveredDate == null)
                Shipments.DeliveredDate = DateTime.Now;

            _context.SaveChanges();
            return RedirectToAction("Details", new { id });
        }
    }
}
