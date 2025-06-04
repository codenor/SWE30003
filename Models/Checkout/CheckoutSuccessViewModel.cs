namespace ElectronicsStoreAss3.Models
{
    public class CheckoutSuccessViewModel
    {
        public int OrderId { get; set; }
        public string? TrackingNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? OrderTotal { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
    }
}