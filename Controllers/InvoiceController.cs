using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ElectronicsStoreAss3.Data;
using ElectronicsStoreAss3.Models.Invoice;
using ElectronicsStoreAss3.Services;
using ElectronicsStoreAss3.Models;
using System.Security.Claims;

namespace ElectronicsStoreAss3.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(
            AppDbContext context,
            IInvoiceService invoiceService,
            ILogger<InvoiceController> logger)
        {
            _context = context;
            _invoiceService = invoiceService;
            _logger = logger;
        }

        // GET: /Invoice/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Order)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
            {
                return NotFound();
            }

            // Check if the current user has access to this invoice
            if (!await UserCanAccessInvoice(invoice.OrderId))
            {
                return Forbid();
            }

            return View(invoice);
        }

        // GET: /Invoice/ByOrder/5
        public async Task<IActionResult> ByOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Check if the current user has access to this order
            if (!await UserCanAccessInvoice(id.Value))
            {
                return Forbid();
            }

            var invoice = await _invoiceService.GetInvoiceByOrderIdAsync(id.Value);

            if (invoice == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = invoice.InvoiceId });
        }

        // POST: /Invoice/SendEmail/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmail(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            // Check if the current user has access to this invoice
            if (!await UserCanAccessInvoice(invoice.OrderId))
            {
                return Forbid();
            }

            var success = await _invoiceService.SendInvoiceEmailAsync(id);

            if (success)
            {
                TempData["ToastMessage"] = "Invoice sent successfully!";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "Failed to send invoice. Please try again.";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<bool> UserCanAccessInvoice(int orderId)
        {
            // Admin users can access any invoice
            if (User.IsInRole("Admin") || User.IsInRole("Owner"))
            {
                return true;
            }

            // Get the current user's account ID
            if (!User.Identity?.IsAuthenticated == true)
            {
                return false;
            }

            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idStr, out int accountId))
            {
                return false;
            }

            // Check if the order belongs to this user
            var order = await _context.Orders.FindAsync(orderId);
            return order?.AccountId == accountId;
        }
    }
}