using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab1_Project.Models;

namespace Lab1_Project.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly BookingSystemContext _context;

        public StatisticsController(BookingSystemContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin") return Forbid();

            var query = _context.Bookings
                .Include(b => b.Workspace)
                .ThenInclude(w => w.Type)
                .Include(b => b.Status)
                .Where(b => b.Status.StatusName == "Підтверджено");

            if (startDate.HasValue)
                query = query.Where(b => b.StartTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.EndTime <= endDate.Value);

            var bookings = await query.ToListAsync();

            var stats = new StatisticsViewModel
            {
                TotalRevenue = bookings.Sum(b => b.TotalAmount),
                TotalBookings = bookings.Count,
                BookingsByType = bookings
                    .GroupBy(b => b.Workspace.Type.TypeName)
                    .ToDictionary(g => g.Key, g => g.Count()),
                RevenueByWorkspace = bookings
                    .GroupBy(b => b.Workspace.Number)
                    .ToDictionary(g => g.Key, g => g.Sum(b => b.TotalAmount)),
                StartDate = startDate,
                EndDate = endDate
            };

            return View(stats);
        }
    }
}