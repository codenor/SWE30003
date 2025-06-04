using System.ComponentModel.DataAnnotations;

namespace ElectronicsStoreAss3.Models.Account
{
    public enum Role
    {
        Customer,
        Owner
    }

    public class Account
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [EnumDataType(typeof(Role))]
        public Role Role { get; set; } = Role.Customer;
    }
}
