﻿using ElectronicsStoreAss3.Data;
using ElectronicsStoreAss3.Models.Statistics;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStoreAss3.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StatisticsService> _logger;

        public StatisticsService(AppDbContext context, ILogger<StatisticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<StatisticsViewModel> GetSalesStatisticsAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var viewModel = new StatisticsViewModel
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    SalesOverview = await GetSalesOverviewAsync(fromDate, toDate),
                    TopProducts = await GetTopProductsAsync(fromDate, toDate),
                    CategoryPerformance = await GetCategoryPerformanceAsync(fromDate, toDate),
                    DailySales = await GetDailySalesAsync(fromDate, toDate),
                    MonthlySales = await GetMonthlySalesAsync()
                };

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales statistics for period {FromDate} to {ToDate}: {Message}",
                    fromDate, toDate, ex.Message);

                // Return a minimal model with empty collections to prevent null reference exceptions
                return new StatisticsViewModel
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    SalesOverview = new SalesOverview(),
                    TopProducts = new List<ProductPerformance>(),
                    CategoryPerformance = new List<CategoryPerformance>(),
                    DailySales = new List<DailySales>(),
                    MonthlySales = new List<MonthlySales>()
                };
            }
        }

        public async Task<SalesOverview> GetSalesOverviewAsync(DateTime fromDate, DateTime toDate)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate &&
                            (o.Status == "Completed" || o.Status == "Delivered"))
                .ToListAsync();

            var totalRevenue = orders.Sum(o => o.TotalAmount);
            var totalOrders = orders.Count;
            var uniqueCustomers = orders.Where(o => o.AccountId.HasValue)
                .Select(o => o.AccountId)
                .Distinct()
                .Count();
            var totalItemsSold = orders.SelectMany(o => o.OrderItems).Sum(oi => oi.Quantity);
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            // Calculate growth compared to previous period
            var periodLength = (toDate - fromDate).Days;
            var previousFromDate = fromDate.AddDays(-periodLength);
            var previousToDate = fromDate.AddDays(-1);

            var previousOrders = await _context.Orders
                .Where(o => o.OrderDate >= previousFromDate && o.OrderDate <= previousToDate &&
                            (o.Status == "Completed" || o.Status == "Delivered"))
                .ToListAsync();

            var previousRevenue = previousOrders.Sum(o => o.TotalAmount);
            var previousOrderCount = previousOrders.Count;
            var previousCustomers = previousOrders.Where(o => o.AccountId.HasValue)
                .Select(o => o.AccountId)
                .Distinct()
                .Count();

            return new SalesOverview
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalCustomers = uniqueCustomers,
                AverageOrderValue = averageOrderValue,
                TotalItemsSold = totalItemsSold,
                RevenueGrowth = previousRevenue > 0 ? ((totalRevenue - previousRevenue) / previousRevenue) * 100 : 0,
                OrderGrowth = previousOrderCount > 0
                    ? ((totalOrders - previousOrderCount) / (decimal)previousOrderCount) * 100
                    : 0,
                CustomerGrowth = previousCustomers > 0
                    ? ((uniqueCustomers - previousCustomers) / (decimal)previousCustomers) * 100
                    : 0
            };
        }

        public async Task<List<ProductPerformance>> GetTopProductsAsync(DateTime fromDate, DateTime toDate,
            int count = 10)
        {
            try
            {
                // Get all data first without ordering at the database level
                var query = await _context.OrderItems
                    .Include(oi => oi.Product)
                    .Include(oi => oi.Order)
                    .Where(oi => oi.Order.OrderDate >= fromDate && oi.Order.OrderDate <= toDate &&
                                 (oi.Order.Status == "Completed" || oi.Order.Status == "Delivered"))
                    .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.SKU, oi.Product.Category })
                    .Select(g => new ProductPerformance
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name,
                        SKU = g.Key.SKU,
                        Category = g.Key.Category,
                        QuantitySold = g.Sum(oi => oi.Quantity),
                        Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice),
                        OrderCount = g.Count(),
                        AveragePrice = g.Average(oi => oi.UnitPrice)
                    })
                    .ToListAsync();

                // Then sort in memory
                return query
                    .OrderByDescending(p => p.Revenue)
                    .Take(count)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top products for period {FromDate} to {ToDate}", fromDate, toDate);
                return new List<ProductPerformance>();
            }
        }

        public async Task<List<CategoryPerformance>> GetCategoryPerformanceAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var totalRevenue = await _context.OrderItems
                    .Include(oi => oi.Order)
                    .Where(oi => oi.Order.OrderDate >= fromDate && oi.Order.OrderDate <= toDate &&
                                 (oi.Order.Status == "Completed" || oi.Order.Status == "Delivered"))
                    .SumAsync(oi => oi.Quantity * oi.UnitPrice);

                // Get all data first without ordering at the database level
                var categoryPerformance = await _context.OrderItems
                    .Include(oi => oi.Product)
                    .Include(oi => oi.Order)
                    .Where(oi => oi.Order.OrderDate >= fromDate && oi.Order.OrderDate <= toDate &&
                                 (oi.Order.Status == "Completed" || oi.Order.Status == "Delivered"))
                    .GroupBy(oi => oi.Product.Category)
                    .Select(g => new CategoryPerformance
                    {
                        Category = g.Key,
                        ProductCount = g.Select(oi => oi.ProductId).Distinct().Count(),
                        QuantitySold = g.Sum(oi => oi.Quantity),
                        Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice),
                        AveragePrice = g.Average(oi => oi.UnitPrice)
                    })
                    .ToListAsync();

                // Calculate market share
                foreach (var category in categoryPerformance)
                {
                    category.MarketShare = totalRevenue > 0 ? (category.Revenue / totalRevenue) * 100 : 0;
                }

                // Sort in memory
                return categoryPerformance
                    .OrderByDescending(c => c.Revenue)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category performance for period {FromDate} to {ToDate}", fromDate,
                    toDate);
                return new List<CategoryPerformance>();
            }
        }

        public async Task<List<DailySales>> GetDailySalesAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Get data without ordering at database level
                var dailySales = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate &&
                                (o.Status == "Completed" || o.Status == "Delivered"))
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new DailySales
                    {
                        Date = g.Key,
                        Revenue = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count(),
                        ItemsSold = g.SelectMany(o => o.OrderItems).Sum(oi => oi.Quantity)
                    })
                    .ToListAsync();

                // Order in memory
                return dailySales
                    .OrderBy(ds => ds.Date)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily sales for period {FromDate} to {ToDate}", fromDate, toDate);
                return new List<DailySales>();
            }
        }

        public async Task<List<MonthlySales>> GetMonthlySalesAsync(int months = 12)
        {
            try
            {
                var fromDate = DateTime.Now.AddMonths(-months);

                // Get data without ordering at database level
                var monthlySales = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.OrderDate >= fromDate &&
                                (o.Status == "Completed" || o.Status == "Delivered"))
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .Select(g => new MonthlySales
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Revenue = g.Sum(o => o.TotalAmount),
                        OrderCount = g.Count(),
                        ItemsSold = g.SelectMany(o => o.OrderItems).Sum(oi => oi.Quantity)
                    })
                    .ToListAsync();

                // Add month names
                foreach (var month in monthlySales)
                {
                    month.MonthName = new DateTime(month.Year, month.Month, 1).ToString("MMMM yyyy");
                }

                // Order in memory
                return monthlySales
                    .OrderBy(ms => ms.Year)
                    .ThenBy(ms => ms.Month)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly sales for the last {Months} months", months);
                return new List<MonthlySales>();
            }
        }

        public async Task<Dictionary<string, object>> GetDashboardDataAsync()
        {
            var last30Days = DateTime.Now.AddDays(-30);
            var today = DateTime.Now.Date;
            var yesterday = today.AddDays(-1);

            var dashboard = new Dictionary<string, object>();

            // Today's sales
            var todayOrders = await _context.Orders
                .Where(o => o.OrderDate.Date == today)
                .ToListAsync();

            // Yesterday's sales for comparison
            var yesterdayOrders = await _context.Orders
                .Where(o => o.OrderDate.Date == yesterday)
                .ToListAsync();

            // Low stock products
            var lowStockProducts = await _context.Inventory
                .Include(i => i.Product)
                .Where(i => i.StockLevel <= i.LowStockThreshold && i.Product.IsActive)
                .CountAsync();

            // Pending shipments
            var pendingShipments = await _context.Shipments
                .Where(s => s.Status == "Processing")
                .CountAsync();

            dashboard.Add("todayRevenue", todayOrders.Sum(o => o.TotalAmount));
            dashboard.Add("todayOrders", todayOrders.Count);
            dashboard.Add("yesterdayRevenue", yesterdayOrders.Sum(o => o.TotalAmount));
            dashboard.Add("yesterdayOrders", yesterdayOrders.Count);
            dashboard.Add("lowStockCount", lowStockProducts);
            dashboard.Add("pendingShipments", pendingShipments);

            return dashboard;
        }
    }
}