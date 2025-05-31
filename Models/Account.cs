
using System.ComponentModel.DataAnnotations;

namespace ElectronicsStoreAss3.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        public string Role { get; set; } = "Customer";

        // Navigation
        public Customer? Customer { get; set; }

        // TODO: 
        // public Owner? Owner { get; set; }
    }
}
