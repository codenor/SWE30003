using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStoreAss3.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [ForeignKey("Account")]
        public int? AccountId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        public string? OrderNotes { get; set; }

        public DateTime LastModified { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Customer? Customer 
        { 
            get 
            {
                if (_customer == null && AccountId.HasValue)
                {
                    return _customer;
                }
                return _customer;
            }
            set => _customer = value;
        }
        private Customer? _customer;
        public virtual Account? Account { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        
        // One-to-One relationship with Shipment
        public virtual Shipment? Shipment { get; set; }

        // Computed properties
        public decimal SubTotal => OrderItems.Sum(item => item.UnitPrice * item.Quantity);
        public decimal GST => SubTotal * 0.10m;
        public decimal GrandTotal => SubTotal + GST;
        public int TotalItems => OrderItems.Sum(item => item.Quantity);

        // Business logic methods
        public void UpdateStatus(string newStatus)
        {
            Status = newStatus;
            LastModified = DateTime.Now;
        }

        public bool CanBeCancelled()
        {
            return Status == "Pending" || 
                   (Shipment?.Status == "Processing");
        }

        public bool IsCompleted()
        {
            return Status == "Completed" && 
                   Shipment?.Status == "Delivered";
        }
    }
}