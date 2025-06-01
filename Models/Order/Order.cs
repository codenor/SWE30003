using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStoreAss3.Models
{
    public class Order
    {
        public int Id { get; set; }

        [ForeignKey("Account")]
        public int? AccountId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public decimal TotalAmount { get; set; }

        // Navigation
        public Customer? Customer { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public Shipment? Shipments { get; set; }
    }
}
