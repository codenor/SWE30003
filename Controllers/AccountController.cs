using Microsoft.AspNetCore.Mvc;
using ElectronicsStoreAss3.Models;
using ElectronicsStoreAss3.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace ElectronicsStoreAss3.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Account/
        [HttpGet]
        public IActionResult Index()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login");

            return View();
        }

        // GET: /Account/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // GET: /Account/Orders
        [HttpGet]
        public IActionResult Orders()
        {
            return View();
        }

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            // Simulated logout (for now, just redirect)
            return RedirectToAction("Login");
        }


        // GET: /Account/Register/
        public IActionResult Register()
        {
            return View();
        }


        // POST: /Account/Register/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (_context.Accounts.Any(a => a.Email == model.Email))
            {
                TempData["ToastMessage"] = "An account with this email already exists.";
                TempData["ToastType"] = "error";
                return View(model);
            }

            var account = new Account
            {
                Email = model.Email!,
                PasswordHash = new PasswordHasher<object>().HashPassword(null, model.Password!),
                Role = Role.Customer
            };

            _context.Accounts.Add(account);
            _context.SaveChanges();

            var customer = new Customer
            {
                AccountId = account.Id,
                FirstName = model.FirstName!,
                LastName = model.LastName!,
                Mobile = model.Mobile!
            };

            _context.Customers.Add(customer);
            _context.SaveChanges();

            TempData["ToastMessage"] = "Account created successfully. Please log in.";
            TempData["ToastType"] = "success";
            return RedirectToAction("Login");
        }


        // GET: /Account/Login/
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var account = _context.Accounts.FirstOrDefault(a => a.Email == model.Email);
            if (account == null)
            {
                TempData["ToastMessage"] = "Invalid email or password.";
                TempData["ToastType"] = "error";
                return View(model);
            }

            var hasher = new PasswordHasher<object>();
            var result = hasher.VerifyHashedPassword(null, account.PasswordHash, model.Password!);

            if (result != PasswordVerificationResult.Success)
            {
                TempData["ToastMessage"] = "Invalid email or password.";
                TempData["ToastType"] = "error";
                return View(model);
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
        new Claim(ClaimTypes.Email, account.Email),
        new Claim(ClaimTypes.Role, account.Role.ToString())
    };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);

            TempData["ToastMessage"] = "Successfully logged in!";
            TempData["ToastType"] = "success";
            return RedirectToAction("Index", "Account");
        }
        // GET: /Account/ForgotPassword/
        [HttpGet]
        public IActionResult ForgotPassword()
        {

            return View();
        }

        // POST: /Account/ForgotPassword/
        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Generate a random 6-digit code
                string code = "000000"; //GenerateCode();

                // Save code and email in TempData (temporary across redirects)
                TempData["ResetCode"] = code;
                TempData["ResetEmail"] = model.Email;

                // TODO: Send code to user's email

                return RedirectToAction("ValidateCode");
            }

            return View(model);
        }

        // GET: /Account/ValidateCode/
        [HttpGet]
        public IActionResult ValidateCode()
        {
            var email = TempData["ResetEmail"] as string;
            if (email == null)
                return RedirectToAction("ForgotPassword");

            return View(new ValidateCodeViewModel { Email = email });
        }

        // POST: /Account/ValidateCode/
        [HttpPost]
        public IActionResult ValidateCode(ValidateCodeViewModel model)
        {
            if (ModelState.IsValid && TempData["ResetCode"]?.ToString() == model.Code)
            {
                TempData["ResetEmail"] = model.Email;
                return RedirectToAction("ResetPassword");
            }

            ModelState.AddModelError("Code", "Invalid or expired code.");
            return View(model);
        }

        // GET: /Account/ResetPassword/
        [HttpGet]
        public IActionResult ResetPassword()
        {
            var email = TempData["ResetEmail"] as string;
            if (email == null)
                return RedirectToAction("ForgotPassword");

            return View(new ResetPasswordViewModel { Email = email });
        }

        // POST: /Account/ResetPassword/
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Lookup user by email and update their password

                return RedirectToAction("Login");
            }

            return View(model);
        }

        // Helper: Generate 6-digit reset code
        private string GenerateCode()
        {
            var rng = new Random();
            return rng.Next(100000, 999999).ToString();
        }
    }
}
