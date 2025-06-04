using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicsStoreAss3.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [ForeignKey("Account")]
        public int AccountId { get; set; }

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        public string Mobile { get; set; } = null!;

        [StringLength(500)]
        public string? Address { get; set; }

        [ForeignKey("Email")]
        public string? Email { get; set; }

        // Navigation
        public Account Account { get; set; } = null!;
    }
}
