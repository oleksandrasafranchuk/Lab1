using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab1_Project.Models;

namespace Lab1_Project.Controllers;

public class BookingHistoryController : Controller
{
    private readonly BookingSystemContext _context;

    public BookingHistoryController(BookingSystemContext context)
    {
        _context = context;
    }

   
    public async Task<IActionResult> Index()
    {
        string role = HttpContext.Session.GetString("UserRole") ?? "User";
        int? currentUserId = HttpContext.Session.GetInt32("UserId");

        var historyQuery = _context.BookingHistories
            .Include(h => h.Booking)                   
                .ThenInclude(b => b.Workspace)          
            .Include(h => h.StatusTo)             
            .Include(h => h.ChangedByUser)              
            .AsNoTracking();                           

        if (role == "Admin")
        {
            var adminLog = await historyQuery
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();
            return View(adminLog);
        }
        else
        {
           
            var userLog = await historyQuery
                .Where(h => h.Booking.UserId == currentUserId)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();
            return View(userLog);
        }
    }
}