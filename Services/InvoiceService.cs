using Microsoft.EntityFrameworkCore;
using ElectronicsStoreAss3.Data;
using ElectronicsStoreAss3.Models.Invoice;
using ElectronicsStoreAss3.Models;

namespace ElectronicsStoreAss3.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(AppDbContext context, ILogger<InvoiceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Invoice> GenerateInvoiceAsync(int orderId)
        {
            try
            {
                // Check if invoice already exists
                var existingInvoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.OrderId == orderId);

                if (existingInvoice != null)
                {
                    return existingInvoice;
                }

                // Get order details
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    throw new ArgumentException($"Order with ID {orderId} not found");
                }

                // Get customer details if available
                string customerName = "Guest";
                string customerEmail = "";
                string billingAddress = "";

                if (order.AccountId.HasValue)
                {
                    var customer = await _context.Customers
                        .Include(c => c.Account)
                        .FirstOrDefaultAsync(c => c.AccountId == order.AccountId);

                    if (customer != null)
                    {
                        customerName = $"{customer.FirstName} {customer.LastName}";
                        customerEmail = customer.Email ?? customer.Account?.Email ?? "";
                        billingAddress = customer.Address ?? "";
                    }
                }

                // Create invoice
                var invoice = new Invoice
                {
                    OrderId = orderId,
                    InvoiceDate = DateTime.Now,
                    TotalAmount = order.TotalAmount,
                    Status = "Generated",
                    CustomerName = customerName,
                    CustomerEmail = customerEmail,
                    BillingAddress = billingAddress,
                    InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{orderId:D6}",
                    PaidDate = DateTime.Now,
                    PaymentMethod = "Credit Card" // Default payment method
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Invoice {invoice.InvoiceNumber} generated for order {orderId}");
                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating invoice for order {orderId}");
                throw;
            }
        }

        public async Task<Invoice> GetInvoiceByOrderIdAsync(int orderId)
        {
            return await _context.Invoices
                .FirstOrDefaultAsync(i => i.OrderId == orderId);
        }

        public async Task<bool> SendInvoiceEmailAsync(int invoiceId)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Order)
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

                if (invoice == null)
                {
                    _logger.LogWarning($"Invoice {invoiceId} not found for email sending");
                    return false;
                }

                if (string.IsNullOrEmpty(invoice.CustomerEmail))
                {
                    _logger.LogWarning($"No email address available for invoice {invoiceId}");
                    return false;
                }

                // In a real application, this would connect to an email service
                // For now, we'll just log the action and return success
                _logger.LogInformation($"Invoice {invoice.InvoiceNumber} sent to {invoice.CustomerEmail}");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending invoice email for invoice {invoiceId}");
                return false;
            }
        }
    }
} 