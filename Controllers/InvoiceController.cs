
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElectronicsStoreAss3.Data;
using ElectronicsStoreAss3.Services;
using ElectronicsStoreAss3.Models.Invoice;
using QuestPDF.Fluent;
using Microsoft.Extensions.Logging;

namespace ElectronicsStoreAss3.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(AppDbContext context, IInvoiceService invoiceService, ILogger<InvoiceController> logger)
        {
            _context = context;
            _invoiceService = invoiceService;
            _logger = logger;
        }

        // GET: /Invoice/Details/5 (InvoiceId)
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Order).ThenInclude(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
            {
                var fallbackOrder = await _context.Orders.FindAsync(id);
                if (fallbackOrder == null)
                    return NotFound();

                var newInvoice = await _invoiceService.GenerateInvoiceAsync(fallbackOrder.OrderId);
                return RedirectToAction(nameof(Details), new { id = newInvoice.InvoiceId });
            }

            return View(invoice);
        }

        // GET: /Invoice/ByOrder/5 (OrderId)
        public async Task<IActionResult> ByOrder(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByOrderIdAsync(id);
            if (invoice == null)
            {
                invoice = await _invoiceService.GenerateInvoiceAsync(id);
            }

            return RedirectToAction(nameof(Details), new { id = invoice.InvoiceId });
        }

        // GET: /Invoice/Pdf/5 (InvoiceId)
        public async Task<IActionResult> Pdf(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Order).ThenInclude(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
                return NotFound();

            var document = new InvoicePdfDocument(invoice);
            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", $"Invoice-{invoice.InvoiceNumber}.pdf");
        }

        // GET: /Invoice/PdfByOrder/5 (OrderId)
        public async Task<IActionResult> PdfByOrder(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByOrderIdAsync(id);
            if (invoice == null)
                return NotFound();

            var document = new InvoicePdfDocument(invoice);
            var stream = new MemoryStream();
            document.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", $"Invoice-{invoice.InvoiceNumber}.pdf");
        }

        // POST: /Invoice/SendEmail/5
        [HttpPost]
        public async Task<IActionResult> SendEmail(int id)
        {
            var result = await _invoiceService.SendInvoiceEmailAsync(id);

            TempData["ToastMessage"] = result
                ? "Invoice email sent successfully."
                : "Failed to send invoice email.";
            TempData["ToastType"] = result ? "success" : "error";

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
