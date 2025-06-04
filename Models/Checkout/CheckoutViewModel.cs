using System.ComponentModel.DataAnnotations;

namespace ElectronicsStoreAss3.Models
{
    public class CheckoutViewModel
    {
        public ShoppingCartViewModel Cart { get; set; } = null!;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^(?:\+614|04)\d{8}$", ErrorMessage = "Enter a valid Australian mobile number.")]
        public string? Mobile { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500)]
        public string? Address { get; set; }

        // Helper properties
        public bool IsLoggedIn => !string.IsNullOrEmpty(Email);
        public string FullName => $"{FirstName} {LastName}";
    }
}