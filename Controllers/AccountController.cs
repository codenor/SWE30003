using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ElectronicsStoreAss3.Models;

namespace ElectronicsStoreAss3.Controllers;

public class AccountController : Controller
{

    // GET: /Account/
    public string Index()
    {
        return "This might be useful, unsure at this very moment";
    }

    // GET: /Account/Login/
    public IActionResult Login()
    {
        return View();
    }

    // GET: /Account/Register/
        public IActionResult Register()
        {
            return View();
        }
}
