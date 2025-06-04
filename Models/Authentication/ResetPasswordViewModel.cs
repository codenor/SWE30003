using System.ComponentModel.DataAnnotations;

namespace ElectronicsStoreAss3.Models.Authentication
{
    public class ResetPasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }

        public string? Email { get; set; }
    }
}