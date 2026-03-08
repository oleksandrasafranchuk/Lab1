using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Lab1_Project.Models;
using Microsoft.EntityFrameworkCore;
namespace Lab1_Project.Controllers;

public class HomeController : Controller
{
   private readonly ILogger<HomeController> _logger;
private readonly BookingSystemContext _context;

  public HomeController(
    ILogger<HomeController> logger,
    BookingSystemContext context)
{
    _logger = logger;
    _context = context;
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
public IActionResult Index()
{
    
    if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserRole")))
    {
        HttpContext.Session.SetString("UserRole", "User");
        HttpContext.Session.SetInt32("UserId", 2); 
    }
    return View();
}

public IActionResult SetRole(string role, int? id)
{
    if (!string.IsNullOrEmpty(role))
    {
        HttpContext.Session.SetString("UserRole", role);
        HttpContext.Session.SetInt32("UserId", id ?? 2);
    }
    
    var returnUrl = Request.Headers["Referer"].ToString();
    return Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
}
}
