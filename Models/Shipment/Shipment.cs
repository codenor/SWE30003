using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStoreAss3.Models
{
    public class Shipment
    {
        [Key]
        public int ShipmentId { get; set; }

        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Processing";

        [StringLength(100)]
        public string? TrackingNumber { get; set; }

        [StringLength(100)]
        public string? CarrierName { get; set; } = "AWE Express";

        [Required]
        public DateTime EstimatedDeliveryDate { get; set; }

        public DateTime? ShippedDate { get; set; }

        public DateTime? DeliveredDate { get; set; }

        [StringLength(500)]
        public string? DeliveryNotes { get; set; }

        [StringLength(200)]
        public string? ShippingAddress { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Order Order { get; set; } = null!;

        // Computed properties
        public bool IsDelivered => Status.Equals("Delivered", StringComparison.OrdinalIgnoreCase);
        public bool IsInTransit => Status.Equals("In Transit", StringComparison.OrdinalIgnoreCase) || 
                                  Status.Equals("Shipped", StringComparison.OrdinalIgnoreCase);
        public bool IsProcessing => Status.Equals("Processing", StringComparison.OrdinalIgnoreCase);

        // Business logic methods
        public void UpdateStatus(string newStatus, string? notes = null)
        {
            Status = newStatus;
            LastUpdated = DateTime.Now;
            
            switch (newStatus.ToLower())
            {
                case "shipped":
                case "in transit":
                    if (ShippedDate == null)
                        ShippedDate = DateTime.Now;
                    break;
                case "delivered":
                    if (DeliveredDate == null)
                        DeliveredDate = DateTime.Now;
                    break;
            }

            if (!string.IsNullOrEmpty(notes))
                DeliveryNotes = notes;
        }

        public void GenerateTrackingNumber()
        {
            if (string.IsNullOrEmpty(TrackingNumber))
            {
                TrackingNumber = $"AWE{DateTime.Now:yyyyMMdd}{OrderId:D6}";
            }
        }
    }
}