using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ElectronicsStoreAss3.Models;

namespace ElectronicsStoreAss3.Controllers;

public class AccountController : Controller
{

    // GET: /Account/
    public string Index()
    {
        return "This is my default action...";
    }

    // GET: /Account/Login/
    public string Login()
    {
        return "This is the Login action method...";
    }

    // GET: /Account/Register/
        public string Register()
        {
            return "This is the Register action method...";
        }
}
