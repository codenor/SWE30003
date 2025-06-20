using System.ComponentModel.DataAnnotations;

namespace ElectronicsStoreAss3.Models.Authentication
{
    public class ValidateCodeViewModel
    {
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Code { get; set; }
    }
}