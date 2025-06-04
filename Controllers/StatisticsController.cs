using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ElectronicsStoreAss3.Services;
using ElectronicsStoreAss3.Models.Statistics;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStoreAss3.Controllers
{
    [Authorize(Roles = "Owner")]
    public class StatisticsController : Controller
    {
        private readonly IStatisticsService _statisticsService;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(IStatisticsService statisticsService, ILogger<StatisticsController> logger)
        {
            _statisticsService = statisticsService;
            _logger = logger;
        }

        // GET: /Statistics
        public async Task<IActionResult> Index(string timeFrame = "30days")
        {
            try
            {
                // Set ViewBag FIRST - before any potential errors
                SetTimeFrameOptions();
                
                var (fromDate, toDate) = GetDateRange(timeFrame);
                var statistics = await _statisticsService.GetSalesStatisticsAsync(fromDate, toDate);
                statistics.TimeFrame = timeFrame;
                
                return View(statistics);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error loading statistics: {Message}", dbEx.InnerException?.Message ?? dbEx.Message);
                TempData["ToastMessage"] = "Database error loading statistics. Please try again later.";
                TempData["ToastType"] = "error";
                
                // Ensure ViewBag is set even on error
                SetTimeFrameOptions();
                
                // Return empty model with proper timeframe
                var emptyModel = new StatisticsViewModel 
                { 
                    TimeFrame = timeFrame,
                    FromDate = GetDateRange(timeFrame).fromDate,
                    ToDate = GetDateRange(timeFrame).toDate
                };
                return View(emptyModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading statistics: {Message}", ex.Message);
                TempData["ToastMessage"] = "Error loading statistics. Please try again later.";
                TempData["ToastType"] = "error";
                
                // Ensure ViewBag is set even on error
                SetTimeFrameOptions();
                
                // Return empty model with proper timeframe
                var emptyModel = new StatisticsViewModel 
                { 
                    TimeFrame = timeFrame,
                    FromDate = GetDateRange(timeFrame).fromDate,
                    ToDate = GetDateRange(timeFrame).toDate
                };
                return View(emptyModel);
            }
        }

        // GET: /Statistics/Dashboard - API endpoint for real-time dashboard data
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var dashboardData = await _statisticsService.GetDashboardDataAsync();
                return Json(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                return Json(new { error = "Failed to load dashboard data" });
            }
        }

        // GET: /Statistics/Custom - Custom date range
        [HttpGet]
        public async Task<IActionResult> Custom(DateTime? fromDate, DateTime? toDate)
        {
            fromDate ??= DateTime.Now.AddDays(-30);
            toDate ??= DateTime.Now;

            try
            {
                SetTimeFrameOptions();
                
                var statistics = await _statisticsService.GetSalesStatisticsAsync(fromDate.Value, toDate.Value);
                statistics.TimeFrame = "custom";
                statistics.FromDate = fromDate.Value;
                statistics.ToDate = toDate.Value;

                return View("Index", statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading custom statistics");
                TempData["ToastMessage"] = "Error loading statistics";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index");
            }
        }

        // API endpoint for chart data
        [HttpGet]
        public async Task<IActionResult> ChartData(string type, string timeFrame = "30days")
        {
            try
            {
                var (fromDate, toDate) = GetDateRange(timeFrame);

                switch (type.ToLower())
                {
                    case "daily":
                        var dailySales = await _statisticsService.GetDailySalesAsync(fromDate, toDate);
                        return Json(dailySales.Select(d => new
                        {
                            date = d.Date.ToString("yyyy-MM-dd"),
                            revenue = d.Revenue,
                            orders = d.OrderCount
                        }));

                    case "monthly":
                        var monthlySales = await _statisticsService.GetMonthlySalesAsync();
                        return Json(monthlySales.Select(m => new
                        {
                            month = m.MonthName,
                            revenue = m.Revenue,
                            orders = m.OrderCount
                        }));

                    case "categories":
                        var categories = await _statisticsService.GetCategoryPerformanceAsync(fromDate, toDate);
                        return Json(categories.Select(c => new
                        {
                            category = c.Category,
                            revenue = c.Revenue,
                            quantity = c.QuantitySold,
                            marketShare = c.MarketShare
                        }));

                    case "products":
                        var products = await _statisticsService.GetTopProductsAsync(fromDate, toDate, 10);
                        return Json(products.Select(p => new
                        {
                            name = p.ProductName,
                            sku = p.SKU,
                            revenue = p.Revenue,
                            quantity = p.QuantitySold
                        }));

                    default:
                        return Json(new { error = "Invalid chart type" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chart data for type: {Type}", type);
                return Json(new { error = "Failed to load chart data" });
            }
        }

        #region Private Helper Methods

        private void SetTimeFrameOptions()
        {
            ViewBag.TimeFrameOptions = new Dictionary<string, string>
            {
                { "7days", "Last 7 Days" },
                { "30days", "Last 30 Days" },
                { "90days", "Last 3 Months" },
                { "1year", "Last Year" },
                { "custom", "Custom Range" }
            };
        }

        private (DateTime fromDate, DateTime toDate) GetDateRange(string timeFrame)
        {
            var toDate = DateTime.Now;
            var fromDate = timeFrame switch
            {
                "7days" => toDate.AddDays(-7),
                "30days" => toDate.AddDays(-30),
                "90days" => toDate.AddDays(-90),
                "1year" => toDate.AddYears(-1),
                _ => toDate.AddDays(-30)
            };

            return (fromDate, toDate);
        }

        #endregion
    }
}