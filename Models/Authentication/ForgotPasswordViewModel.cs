using System.ComponentModel.DataAnnotations;

namespace ElectronicsStoreAss3.Models.Authentication
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
