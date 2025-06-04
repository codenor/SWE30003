using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ElectronicsStoreAss3.Models;

namespace ElectronicsStoreAss3.Models.Invoice
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }
        
        public int OrderId { get; set; }
        
        [ForeignKey("OrderId")]
        public virtual Order.Order Order { get; set; }
        
        [Required]
        public DateTime InvoiceDate { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        [Required]
        public string Status { get; set; } = "Generated";
        
        public string? CustomerName { get; set; }
        
        public string? CustomerEmail { get; set; }
        
        public string? BillingAddress { get; set; }
        
        public string? InvoiceNumber { get; set; }
        
        public DateTime? PaidDate { get; set; }
        
        public string? PaymentMethod { get; set; }
    }
}