using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElectronicsStoreAss3.Data;
using ElectronicsStoreAss3.Models;
using ElectronicsStoreAss3.Models.Order;
using ElectronicsStoreAss3.Services;

namespace ElectronicsStoreAss3.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            AppDbContext context, 
            IOrderService orderService, 
            ILogger<OrderController> logger)
        {
            _context = context;
            _orderService = orderService;
            _logger = logger;
        }

        // GET: /Order/
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        // GET: /Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // POST: /Order/Finalize/5 — updates inventory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalize(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

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

            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = $"Inventory updated for Order #{order.OrderId}.";
            TempData["ToastType"] = "success";

            return RedirectToAction("Details", new { id = order.OrderId });
        }

        // POST: /Order/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            // Use the order service to cancel the order
            bool success = await _orderService.CancelOrderAsync(id);

            if (!success)
            {
                _logger.LogWarning("Failed to cancel order with ID {OrderId}", id);
                TempData["ToastMessage"] = $"Order #{id} could not be cancelled.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Details", new { id = id });
            }

            // After cancellation, return items to inventory
            var order = await _orderService.GetOrderByIdAsync(id);
            foreach (var item in order.OrderItems)
            {
                var inventory = _context.Inventory.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (inventory != null)
                {
                    inventory.StockLevel += item.Quantity;
                    inventory.LastUpdated = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            TempData["ToastMessage"] = $"Order #{id} has been cancelled successfully.";
            TempData["ToastType"] = "success";

            // If this is called from Account/Orders, redirect back there
            if (Request.Headers["Referer"].ToString().Contains("/Account/Orders"))
                return Redirect(Request.Headers["Referer"].ToString());

            return RedirectToAction("Index");
        }
    }
}