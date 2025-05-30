using Microsoft.AspNetCore.Mvc;
using ElectronicsStoreAss3.Models;

namespace ElectronicsStoreAss3.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/
        public IActionResult  Index()
        {
             if (!User.Identity?.IsAuthenticated ?? true)
             {
                return RedirectToAction("Login");
             }

             return View();
        }

        // GET: /Account/Register/
        public IActionResult Register()
        {
            return View();
        }

        // GET: /Account/Login/
        public IActionResult Login()
        {
            return View();
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
