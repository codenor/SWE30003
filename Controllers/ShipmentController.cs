using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ElectronicsStoreAss3.Services;
using ElectronicsStoreAss3.Models;
using System.Security.Claims;

namespace ElectronicsStoreAss3.Controllers
{
    public class ShipmentController : Controller
    {
        private readonly IShipmentService _shipmentService;
        private readonly ILogger<ShipmentController> _logger;

        public ShipmentController(IShipmentService shipmentService, ILogger<ShipmentController> logger)
        {
            _shipmentService = shipmentService;
            _logger = logger;
        }

        // GET: /Shipment - Admin only
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var shipments = await _shipmentService.GetAllShipmentsAsync();
                return View(shipments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shipments list");
                TempData["ToastMessage"] = "Error loading shipments";
                TempData["ToastType"] = "error";
                return View(Enumerable.Empty<Shipment>());
            }
        }

        // GET: /Shipment/Details/5 - Accessible by owner or customer who owns the order
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var shipment = await _shipmentService.GetShipmentByIdAsync(id);
                if (shipment == null)
                {
                    TempData["ToastMessage"] = "Shipment not found";
                    TempData["ToastType"] = "error";
                    return NotFound();
                }

                // Authorization check - customer can only view their own shipments
                if (!User.IsInRole("Owner"))
                {
                    var accountId = GetCurrentAccountId();
                    if (accountId == null || shipment.Order.AccountId != accountId)
                    {
                        return Forbid();
                    }
                }

                return View(shipment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shipment details for ID {ShipmentId}", id);
                TempData["ToastMessage"] = "Error loading shipment details";
                TempData["ToastType"] = "error";
                return NotFound();
            }
        }

        // GET: /Shipment/Track?trackingNumber=AWE20241201000001
        // or /track/{trackingNumber}
        [AllowAnonymous]
        public async Task<IActionResult> Track(string? trackingNumber)
        {
            // If no tracking number provided, just show the empty form
            if (string.IsNullOrWhiteSpace(trackingNumber))
            {
                return View(null);
            }

            try
            {
                var shipment = await _shipmentService.GetShipmentByTrackingNumberAsync(trackingNumber);
                if (shipment == null)
                {
                    ViewBag.Error = "Tracking number not found. Please check and try again.";
                    return View();
                }

                return View("TrackingDetails", shipment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking shipment with number {TrackingNumber}", trackingNumber);
                ViewBag.Error = "Error retrieving tracking information. Please try again.";
                return View();
            }
        }

        // POST: /Shipment/UpdateStatus - Admin only
        [HttpPost]
        [Authorize(Roles = "Owner")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int shipmentId, string newStatus, string? notes = null)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
            {
                TempData["ToastMessage"] = "Status cannot be empty";
                TempData["ToastType"] = "error";
                return RedirectToAction("Details", new { id = shipmentId });
            }

            try
            {
                var success = await _shipmentService.UpdateShipmentStatusAsync(shipmentId, newStatus, notes);

                if (success)
                {
                    TempData["ToastMessage"] = $"Shipment status updated to '{newStatus}'";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "Failed to update shipment status";
                    TempData["ToastType"] = "error";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for shipment {ShipmentId}", shipmentId);
                TempData["ToastMessage"] = "Error updating shipment status";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction("Details", new { id = shipmentId });
        }

        // POST: /Shipment/AssignTracking - Admin only
        [HttpPost]
        [Authorize(Roles = "Owner")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTracking(int shipmentId, string trackingNumber, string? carrierName)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
            {
                TempData["ToastMessage"] = "Tracking number cannot be empty";
                TempData["ToastType"] = "error";
                return RedirectToAction("Details", new { id = shipmentId });
            }

            try
            {
                var success = await _shipmentService.AssignTrackingNumberAsync(shipmentId, trackingNumber, carrierName);

                if (success)
                {
                    TempData["ToastMessage"] = "Tracking number assigned successfully";
                    TempData["ToastType"] = "success";
                }
                else
                {
                    TempData["ToastMessage"] = "Failed to assign tracking number - it may already be in use";
                    TempData["ToastType"] = "error";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning tracking number to shipment {ShipmentId}", shipmentId);
                TempData["ToastMessage"] = "Error assigning tracking number";
                TempData["ToastType"] = "error";
            }

            return RedirectToAction("Details", new { id = shipmentId });
        }

        // GET: /Shipment/Pending - Admin only, shows shipments that need attention
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Pending()
        {
            try
            {
                var pendingShipments = await _shipmentService.GetPendingShipmentsAsync();
                return View(pendingShipments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pending shipments");
                TempData["ToastMessage"] = "Error loading pending shipments";
                TempData["ToastType"] = "error";
                return View(Enumerable.Empty<Shipment>());
            }
        }

        // GET: /Shipment/Overdue - Admin only, shows overdue shipments
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Overdue()
        {
            try
            {
                var overdueShipments = await _shipmentService.GetOverdueShipmentsAsync();
                return View(overdueShipments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading overdue shipments");
                TempData["ToastMessage"] = "Error loading overdue shipments";
                TempData["ToastType"] = "error";
                return View(Enumerable.Empty<Shipment>());
            }
        }

        // GET: /Shipment/MyShipments - Customer shipments
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyShipments()
        {
            try
            {
                var accountId = GetCurrentAccountId();
                if (accountId == null)
                {
                    return Forbid();
                }

                var customerShipments = await _shipmentService.GetShipmentsByCustomerAsync(accountId.Value);
                return View(customerShipments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer shipments");
                TempData["ToastMessage"] = "Error loading your shipments";
                TempData["ToastType"] = "error";
                return View(Enumerable.Empty<Shipment>());
            }
        }

        // AJAX endpoint for quick status updates
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> QuickStatusUpdate(int shipmentId, string status)
        {
            try
            {
                var success = await _shipmentService.UpdateShipmentStatusAsync(shipmentId, status);
                return Json(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in quick status update for shipment {ShipmentId}", shipmentId);
                return Json(new { success = false });
            }
        }

        //#region Private Helper Methods

        /// <summary>
        /// Gets the current authenticated user's account ID
        /// </summary>
        /// <returns>Account ID if user is authenticated, null otherwise</returns>
        private int? GetCurrentAccountId()
        {
            if (!User.Identity?.IsAuthenticated == true)
                return null;

            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idStr, out int id) ? id : null;
        }
    }
}