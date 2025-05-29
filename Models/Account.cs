using System.ComponentModel.DataAnnotations;

namespace ElectronicsStoreAss3.Models;

public class Account {
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string? FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string? LastName { get; set; }

    [Required]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.(com|com\.au)$", ErrorMessage = "Enter a valid email ending in .com or .com.au.")]
    public string? Email { get; set; }

    [Required]
    [RegularExpression(@"^(?:\+614|04)\d{8}$", ErrorMessage = "Enter a valid Australian mobile number.")]
    public string? Mobile { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string? Password { get; set; }
}
