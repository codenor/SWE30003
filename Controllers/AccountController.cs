using Microsoft.AspNetCore.Mvc;
using ElectronicsStoreAss3.Models;
using ElectronicsStoreAss3.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ElectronicsStoreAss3.Models.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsStoreAss3.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Account
        [HttpGet]
        public IActionResult Index()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login", "Authentication");

            int accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var account = _context.Accounts.FirstOrDefault(a => a.Id == accountId);

            if (account == null)
            {
                TempData["ToastMessage"] = "⚠️ Account not found. Please log in again.";
                return RedirectToAction("Login", "Authentication");
            }

            switch (account.Role)
            {
                case Role.Customer:
                    var customer = _context.Customers.FirstOrDefault(c => c.AccountId == accountId);
                    if (customer == null)
                    {
                        TempData["ToastMessage"] = "⚠️ Customer profile not found.";
                        return RedirectToAction("Index");
                    }

                    ViewBag.Role = "Customer";
                    customer.Email = account.Email;
                    return View(customer);

                case Role.Owner:
                    var owner = _context.Owners.FirstOrDefault(o => o.AccountId == accountId);
                    if (owner == null)
                    {
                        TempData["ToastMessage"] = "⚠️ Owner profile not found.";
                        return RedirectToAction("Index");
                    }

                    ViewBag.Role = "Owner";
                    owner.Email = account.Email;
                    return View(owner);

                default:
                    TempData["ToastMessage"] = "⚠️ Unsupported account role.";
                    return RedirectToAction("Index");
            }
        }

        // POST: /Account/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login", "Authentication");

            int accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var account = _context.Accounts.FirstOrDefault(a => a.Id == accountId);

            if (account == null)
            {
                TempData["ToastMessage"] = "⚠️ Account not found.";
                return RedirectToAction("Index");
            }

            string role = Request.Form["Role"];
            string newEmail = Request.Form["Email"];

            account.Email = newEmail;
            _context.Accounts.Update(account);

            if (role == "Customer")
            {
                var customer = _context.Customers.FirstOrDefault(c => c.AccountId == accountId);
                if (customer != null)
                {
                    customer.FirstName = Request.Form["FirstName"];
                    customer.LastName = Request.Form["LastName"];
                    customer.Mobile = Request.Form["Mobile"];
                    customer.Email = newEmail;
                    customer.Address = Request.Form["Address"];
                    _context.Customers.Update(customer);
                }
            }
            else if (role == "Owner")
            {
                var owner = _context.Owners.FirstOrDefault(o => o.AccountId == accountId);
                if (owner != null)
                {
                    owner.Name = Request.Form["Name"];
                    owner.LastName = Request.Form["LastName"];
                    owner.Email = newEmail;
                    _context.Owners.Update(owner);
                }
            }

            _context.SaveChanges();
            TempData["ToastMessage"] = "✅ Profile updated successfully.";
            return RedirectToAction("Index");
        }

        // GET: /Account/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string CurrentPassword, string NewPassword)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login", "Authentication");

            int accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var account = _context.Accounts.FirstOrDefault(a => a.Id == accountId);

            if (account == null)
            {
                TempData["ToastMessage"] = "⚠️ Account not found.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index");
            }

            var hasher = new PasswordHasher<object>();
            var currentCheck = hasher.VerifyHashedPassword(null, account.PasswordHash, CurrentPassword);

            if (currentCheck != PasswordVerificationResult.Success)
            {
                TempData["ToastMessage"] = "❌ Current password is incorrect.";
                TempData["ToastType"] = "error";
                return RedirectToAction("ChangePassword");
            }

            var reuseCheck = hasher.VerifyHashedPassword(null, account.PasswordHash, NewPassword);
            if (reuseCheck == PasswordVerificationResult.Success)
            {
                TempData["ToastMessage"] = "❌ New password must be different from the current password.";
                TempData["ToastType"] = "error";
                return RedirectToAction("ChangePassword");
            }

            account.PasswordHash = hasher.HashPassword(null, NewPassword);
            _context.Accounts.Update(account);
            _context.SaveChanges();

            TempData["ToastMessage"] = "✅ Password changed successfully.";
            TempData["ToastType"] = "success";
            return RedirectToAction("Index");
        }

        // GET: /Account/Orders
        [HttpGet]
        public IActionResult Orders()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login", "Authentication");

            int accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Shipment)
                .Where(o => o.AccountId == accountId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Sign the user out from the cookie authentication scheme
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Optionally clear session if you're also using session storage
            HttpContext.Session.Clear();

            // Redirect to login or home page
            return RedirectToAction("Login", "Authentication");
        }
    }
}