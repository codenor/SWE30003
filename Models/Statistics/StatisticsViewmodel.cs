using System.ComponentModel.DataAnnotations;

namespace ElectronicsStoreAss3.Models.Statistics
{
    public class StatisticsViewModel
    {
        public SalesOverview SalesOverview { get; set; } = new();
        public List<ProductPerformance> TopProducts { get; set; } = new();
        public List<CategoryPerformance> CategoryPerformance { get; set; } = new();
        public List<DailySales> DailySales { get; set; } = new();
        public List<MonthlySales> MonthlySales { get; set; } = new();
        
        public DateTime FromDate { get; set; } = DateTime.Now.AddDays(-30);
        public DateTime ToDate { get; set; } = DateTime.Now;
        public string TimeFrame { get; set; } = "30days";
    }

    public class SalesOverview
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalItemsSold { get; set; }
        
        // Comparison with previous period
        public decimal RevenueGrowth { get; set; }
        public decimal OrderGrowth { get; set; }
        public decimal CustomerGrowth { get; set; }
    }

    public class ProductPerformance
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public decimal AveragePrice { get; set; }
    }

    public class CategoryPerformance
    {
        public string Category { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal MarketShare { get; set; }
    }

    public class DailySales
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public int ItemsSold { get; set; }
    }

    public class MonthlySales
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public int ItemsSold { get; set; }
    }
}
