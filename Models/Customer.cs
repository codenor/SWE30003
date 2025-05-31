
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
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        [Required]
        [RegularExpression(@"^(?:\+614|04)\d{8}$", ErrorMessage = "Enter a valid Australian mobile number.")]
        public string Mobile { get; set; } = null!;

        // Navigation
        public Account Account { get; set; } = null!;
    }
}
