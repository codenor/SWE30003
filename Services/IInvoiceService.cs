using ElectronicsStoreAss3.Models.Invoice;

namespace ElectronicsStoreAss3.Services
{
    public interface IInvoiceService
    {
        Task<Invoice> GenerateInvoiceAsync(int orderId);
        Task<Invoice> GetInvoiceByOrderIdAsync(int orderId);
        Task<bool> SendInvoiceEmailAsync(int invoiceId);
    }
} 