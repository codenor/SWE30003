using System.ComponentModel.DataAnnotations;

namespace ElectronicsStoreAss3.Models
{
    public class ValidateCodeViewModel
    {
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Code { get; set; }
    }
}