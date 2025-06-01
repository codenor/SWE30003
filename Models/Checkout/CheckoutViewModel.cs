namespace ElectronicsStoreAss3.Models
{
    public class CheckoutViewModel
    {
        public ShoppingCartViewModel Cart { get; set; } = null!;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Mobile { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }
    }


}
