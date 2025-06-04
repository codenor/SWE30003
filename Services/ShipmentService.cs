using ElectronicsStoreAss3.Data;
using ElectronicsStoreAss3.Models;
using ElectronicsStoreAss3.Models.Order;
using ElectronicsStoreAss3.Models.Shipment;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStoreAss3.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ShipmentService> _logger;

        public ShipmentService(AppDbContext context, ILogger<ShipmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Shipment?> GetShipmentByIdAsync(int shipmentId)
        {
            return await _context.Shipments
                .Include(s => s.Order)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
                .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId);
        }

        public async Task<Shipment?> GetShipmentByOrderIdAsync(int orderId)
        {
            return await _context.Shipments
                .Include(s => s.Order)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
                .FirstOrDefaultAsync(s => s.OrderId == orderId);
        }

        public async Task<Shipment?> GetShipmentByTrackingNumberAsync(string trackingNumber)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
                return null;

            return await _context.Shipments
                .Include(s => s.Order)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
                .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
        }

        public async Task<IEnumerable<Shipment>> GetAllShipmentsAsync()
        {
            return await _context.Shipments
                .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Shipment>> GetShipmentsByStatusAsync(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return Enumerable.Empty<Shipment>();

            return await _context.Shipments
                .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
                .Where(s => s.Status.ToLower() == status.ToLower())
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> CreateShipmentAsync(int orderId, string? shippingAddress = null)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    _logger.LogWarning("Cannot create shipment - Order {OrderId} not found", orderId);
                    return false;
                }

                // Check if shipment already exists
                var existingShipment = await _context.Shipments
                    .FirstOrDefaultAsync(s => s.OrderId == orderId);

                if (existingShipment != null)
                {
                    _logger.LogWarning("Shipment already exists for Order {OrderId}", orderId);
                    return false;
                }

                var shipment = new Shipment
                {
                    OrderId = orderId,
                    Status = "Processing",
                    EstimatedDeliveryDate = CalculateEstimatedDeliveryDate(),
                    ShippingAddress = shippingAddress ?? order.Customer?.Address ?? "Address not provided",
                    CreatedDate = DateTime.Now,
                    LastUpdated = DateTime.Now,
                    CarrierName = "AWE Express"
                };

                shipment.GenerateTrackingNumber();

                _context.Shipments.Add(shipment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Shipment created for Order {OrderId} with tracking {TrackingNumber}",
                    orderId, shipment.TrackingNumber);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shipment for Order {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> UpdateShipmentStatusAsync(int shipmentId, string newStatus, string? notes = null)
        {
            try
            {
                var shipment = await _context.Shipments
                    .Include(s => s.Order)
                    .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId);

                if (shipment == null)
                {
                    _logger.LogWarning("Cannot update status - Shipment {ShipmentId} not found", shipmentId);
                    return false;
                }

                var oldStatus = shipment.Status;
                shipment.UpdateStatus(newStatus, notes);

                // Update related order status
                UpdateOrderStatusBasedOnShipment(shipment.Order, newStatus);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Shipment {ShipmentId} status updated from {OldStatus} to {NewStatus}",
                    shipmentId, oldStatus, newStatus);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shipment {ShipmentId} status to {Status}",
                    shipmentId, newStatus);
                return false;
            }
        }

        public async Task<bool> AssignTrackingNumberAsync(int shipmentId, string trackingNumber,
            string? carrierName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(trackingNumber))
                {
                    _logger.LogWarning("Cannot assign empty tracking number to shipment {ShipmentId}", shipmentId);
                    return false;
                }

                var shipment = await _context.Shipments.FindAsync(shipmentId);
                if (shipment == null)
                {
                    _logger.LogWarning("Cannot assign tracking - Shipment {ShipmentId} not found", shipmentId);
                    return false;
                }

                // Check if tracking number is already in use
                var existingShipment = await _context.Shipments
                    .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber && s.ShipmentId != shipmentId);

                if (existingShipment != null)
                {
                    _logger.LogWarning("Tracking number {TrackingNumber} already exists", trackingNumber);
                    return false;
                }

                shipment.TrackingNumber = trackingNumber;
                shipment.CarrierName = carrierName ?? shipment.CarrierName;
                shipment.LastUpdated = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Tracking number {TrackingNumber} assigned to shipment {ShipmentId}",
                    trackingNumber, shipmentId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning tracking number to shipment {ShipmentId}", shipmentId);
                return false;
            }
        }

        public async Task<IEnumerable<Shipment>> GetPendingShipmentsAsync()
        {
            return await _context.Shipments
                .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
                .Where(s => s.Status == "Processing")
                .OrderBy(s => s.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Shipment>> GetOverdueShipmentsAsync()
        {
            var cutoffDate = DateTime.Now;
            return await _context.Shipments
                .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
                .Where(s => s.EstimatedDeliveryDate < cutoffDate && s.Status != "Delivered")
                .OrderBy(s => s.EstimatedDeliveryDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Shipment>> GetShipmentsByCustomerAsync(int accountId)
        {
            return await _context.Shipments
                .Include(s => s.Order)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(s => s.Order.AccountId == accountId)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
        }

        #region Private Helper Methods

        private static DateTime CalculateEstimatedDeliveryDate()
        {
            var startDate = DateTime.Now;
            var businessDays = 5; // Standard delivery time

            return startDate.AddBusinessDays(businessDays);
        }

        private static void UpdateOrderStatusBasedOnShipment(Order order, string shipmentStatus)
        {
            switch (shipmentStatus.ToLowerInvariant())
            {
                case "shipped":
                case "in transit":
                    order.UpdateStatus("Shipped");
                    break;
                case "delivered":
                    order.UpdateStatus("Completed");
                    break;
                case "failed":
                case "returned":
                    order.UpdateStatus("Failed");
                    break;
            }
        }

        #endregion
    }
}

// Extension methods for business day calculations
public static class DateTimeExtensions
{
    public static DateTime AddBusinessDays(this DateTime startDate, int businessDays)
    {
        if (businessDays == 0) return startDate;

        int direction = Math.Sign(businessDays);
        int businessDaysRemaining = Math.Abs(businessDays);
        DateTime current = startDate;

        while (businessDaysRemaining > 0)
        {
            current = current.AddDays(direction);

            // Skip weekends
            if (current.DayOfWeek != DayOfWeek.Saturday &&
                current.DayOfWeek != DayOfWeek.Sunday)
            {
                businessDaysRemaining--;
            }
        }

        return current;
    }

    public static bool IsBusinessDay(this DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday &&
               date.DayOfWeek != DayOfWeek.Sunday;
    }
}