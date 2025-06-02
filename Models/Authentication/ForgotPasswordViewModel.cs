using System.ComponentModel.DataAnnotations;

namespace ElectronicsStoreAss3.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
