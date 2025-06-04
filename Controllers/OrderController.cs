using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElectronicsStoreAss3.Data;
using ElectronicsStoreAss3.Models;


namespace ElectronicsStoreAss3.Controllers

{
    public class OrderController : Controller

    {
        private readonly AppDbContext _context;


        public OrderController(AppDbContext context)

        {
            _context = context;
        }


        // GET: /Order/

        public IActionResult Index()

        {
            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.Shipment)
                .OrderByDescending(o => o.OrderDate)
                .ToList();


            return View(orders);
        }


        // GET: /Order/Details/5

        public IActionResult Details(int id)

        {
            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.Shipment)
                .FirstOrDefault(o => o.OrderId == id);


            if (order == null)

                return NotFound();


            return View(order);
        }


        // POST: /Order/Finalize/5 — updates inventory

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Finalize(int id)

        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.OrderId == id);


            if (order == null)

                return NotFound();


            foreach (var item in order.OrderItems)

            {
                var inventory = _context.Inventory.FirstOrDefault(i => i.ProductId == item.ProductId);

                if (inventory != null)

                {
                    inventory.StockLevel -= item.Quantity;

                    inventory.LastUpdated = DateTime.Now;


                    if (inventory.StockLevel < 0)

                        inventory.StockLevel = 0;
                }
            }


            _context.SaveChanges();


            TempData["ToastMessage"] = $"Inventory updated for Order #{order.OrderId}.";

            TempData["ToastType"] = "success";


            return RedirectToAction("Details", new { id = order.OrderId });
        }

        // POST: /Order/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Shipment)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            // Check if the order can be cancelled
            if (!order.CanBeCancelled())
            {
                TempData["ToastMessage"] = $"Order #{order.OrderId} cannot be cancelled in its current state.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Details", new { id = order.OrderId });
            }

            // Update order status
            order.Status = "Cancelled";
            order.LastModified = DateTime.Now;
            
            // Update shipment status if exists
            if (order.Shipment != null)
            {
                order.Shipment.Status = "Cancelled";
                order.Shipment.LastUpdated = DateTime.Now;
                order.Shipment.DeliveryNotes = (order.Shipment.DeliveryNotes ?? "") + "\nOrder cancelled by customer on " + DateTime.Now.ToString("MMM dd, yyyy");
            }

            // Return items to inventory
            foreach (var item in order.OrderItems)
            {
                var inventory = _context.Inventory.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (inventory != null)
                {
                    inventory.StockLevel += item.Quantity;
                    inventory.LastUpdated = DateTime.Now;
                }
            }

            _context.SaveChanges();

            TempData["ToastMessage"] = $"Order #{order.OrderId} has been cancelled successfully.";
            TempData["ToastType"] = "success";

            // If this is called from Account/Orders, redirect back there
            if (Request.Headers["Referer"].ToString().Contains("/Account/Orders"))
                return Redirect(Request.Headers["Referer"].ToString());

            return RedirectToAction("Index");
        }
    }
}