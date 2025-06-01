using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStoreAss3.Models
{
    public class Shipment
    {
        public int Id { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [Required]
        public string Status { get; set; } = "Processing";

        [Required]
        public DateTime EstimatedDeliveryDate { get; set; }

        public DateTime? DeliveredDate { get; set; }

        // TODO: 
        // public Order Order { get; set; } = null!;
    }
}
