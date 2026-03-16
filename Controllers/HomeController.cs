using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Lab1_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab1_Project.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly BookingSystemContext _context;

    public HomeController(ILogger<HomeController> logger, BookingSystemContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole")))
        {
            var defaultUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == 2) 
                              ?? await _context.Users.FirstOrDefaultAsync();

            if (defaultUser != null)
            {
                HttpContext.Session.SetString("UserRole", "User");
                HttpContext.Session.SetInt32("UserId", defaultUser.Id);
            }
        }
        return View();
    }

    public async Task<IActionResult> SetRole(string role, int? id)
    {
        if (!string.IsNullOrEmpty(role) && id.HasValue)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == id.Value);
            
            if (userExists)
            {
                HttpContext.Session.SetString("UserRole", role);
                HttpContext.Session.SetInt32("UserId", id.Value);
            }
            else
            {

                TempData["Error"] = $"Помилка: Користувача з ID {id} не існує в системі!";
            }
        }
        
        var returnUrl = Request.Headers["Referer"].ToString();
        return Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}