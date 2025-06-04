using ElectronicsStoreAss3.Models;

namespace ElectronicsStoreAss3.Services
{
    public interface IShipmentService
    {
        // Basic CRUD operations
        Task<Shipment?> GetShipmentByIdAsync(int shipmentId);
        Task<Shipment?> GetShipmentByOrderIdAsync(int orderId);
        Task<Shipment?> GetShipmentByTrackingNumberAsync(string trackingNumber);
        Task<IEnumerable<Shipment>> GetAllShipmentsAsync();
        Task<IEnumerable<Shipment>> GetShipmentsByStatusAsync(string status);
        
        // Customer-specific operations
        Task<IEnumerable<Shipment>> GetShipmentsByCustomerAsync(int accountId);
        
        // Shipment management
        Task<bool> CreateShipmentAsync(int orderId, string? shippingAddress = null);
        Task<bool> UpdateShipmentStatusAsync(int shipmentId, string newStatus, string? notes = null);
        Task<bool> AssignTrackingNumberAsync(int shipmentId, string trackingNumber, string? carrierName = null);
        
        // Administrative queries
        Task<IEnumerable<Shipment>> GetPendingShipmentsAsync();
        Task<IEnumerable<Shipment>> GetOverdueShipmentsAsync();
    }
}