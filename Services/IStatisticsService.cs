using ElectronicsStoreAss3.Models.Statistics;

namespace ElectronicsStoreAss3.Services
{
    public interface IStatisticsService
    {
        Task<StatisticsViewModel> GetSalesStatisticsAsync(DateTime fromDate, DateTime toDate);
        Task<SalesOverview> GetSalesOverviewAsync(DateTime fromDate, DateTime toDate);
        Task<List<ProductPerformance>> GetTopProductsAsync(DateTime fromDate, DateTime toDate, int count = 10);
        Task<List<CategoryPerformance>> GetCategoryPerformanceAsync(DateTime fromDate, DateTime toDate);
        Task<List<DailySales>> GetDailySalesAsync(DateTime fromDate, DateTime toDate);
        Task<List<MonthlySales>> GetMonthlySalesAsync(int months = 12);
        Task<Dictionary<string, object>> GetDashboardDataAsync();
    }
}
