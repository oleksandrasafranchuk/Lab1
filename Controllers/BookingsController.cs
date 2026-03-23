using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab1_Project.Models;

namespace Lab1_Project.Controllers;

public class BookingsController : Controller
{
    private readonly BookingSystemContext _context;

    public BookingsController(BookingSystemContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? statusId, string workspaceNumber, DateTime? dateFrom)
    {
        string role = HttpContext.Session.GetString("UserRole") ?? "User";
        int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 2;

        var query = _context.Bookings
            .Include(b => b.Workspace)
            .Include(b => b.Status)
            .Include(b => b.User)
            .AsQueryable();

        if (role != "Admin")
        {
            query = query.Where(b => b.UserId == currentUserId);
        }

        if (statusId.HasValue)
            query = query.Where(b => b.StatusId == statusId);

        if (!string.IsNullOrEmpty(workspaceNumber))
            query = query.Where(b => b.Workspace.Number.Contains(workspaceNumber));

        if (dateFrom.HasValue)
            query = query.Where(b => b.StartTime.Date >= dateFrom.Value.Date);

        ViewBag.StatusId = new SelectList(_context.BookingStatuses, "Id", "StatusName");
        
        var result = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
        return View(result);
    }

    public IActionResult Create(int? workspaceId)
    {
        var booking = new Booking
        {
            StartTime = DateTime.Now.AddMinutes(5), 
            EndTime = DateTime.Now.AddHours(1)
        };

        if (workspaceId.HasValue)
        {
            booking.WorkspaceId = workspaceId.Value;
            ViewBag.PricePerHour = _context.Workspaces.Find(workspaceId)?.PricePerHour ?? 0;
        }

        ViewBag.WorkspaceId = new SelectList(_context.Workspaces.Where(w => w.IsActive), "Id", "Number", workspaceId);
        return View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Booking booking, string userComment)
    {
        ModelState.Remove("User");
        ModelState.Remove("Workspace");
        ModelState.Remove("Status");
        ModelState.Remove("BookingHistories");

        int? userId = HttpContext.Session.GetInt32("UserId");
        
        if (userId == null || !await _context.Users.AnyAsync(u => u.Id == userId))
        {
            ModelState.AddModelError("", "Помилка авторизації. Спробуйте увійти знову.");
        }

        bool isOccupied = await _context.Bookings.AnyAsync(b =>
            b.WorkspaceId == booking.WorkspaceId &&
            b.StatusId != 3 &&
            ((booking.StartTime < b.EndTime) && (booking.EndTime > b.StartTime)));

        if (isOccupied)
        {
            ModelState.AddModelError("", "Це місце вже заброньовано на обраний час.");
        }

        var duration = (decimal)(booking.EndTime - booking.StartTime).TotalHours;
        if (duration <= 0)
        {
            ModelState.AddModelError("", "Час завершення має бути пізнішим за час початку.");
        }

        var workspace = await _context.Workspaces.FindAsync(booking.WorkspaceId);
        if (!ModelState.IsValid)
        {
            ViewBag.PricePerHour = workspace?.PricePerHour ?? 0;
            ViewBag.WorkspaceId = new SelectList(_context.Workspaces.Where(w => w.IsActive), "Id", "Number", booking.WorkspaceId);
            return View(booking); 
        }

        if (!await _context.BookingStatuses.AnyAsync(s => s.Id == 1))
        {
            ModelState.AddModelError("", "Системна помилка: статус 'Очікує підтвердження' не знайдено.");
            ViewBag.PricePerHour = workspace?.PricePerHour ?? 0;
            ViewBag.WorkspaceId = new SelectList(_context.Workspaces.Where(w => w.IsActive), "Id", "Number", booking.WorkspaceId);
            return View(booking);
        }

        booking.TotalAmount = duration * workspace!.PricePerHour;
        booking.StatusId = 1;
        booking.UserId = userId.Value;
        booking.CreatedAt = DateTime.Now;

        _context.Add(booking);
        await _context.SaveChangesAsync();

        _context.BookingHistories.Add(new BookingHistory
        {
            BookingId = booking.Id,
            StatusToId = 1,
            ChangedByUserId = userId.Value,
            ChangedAt = DateTime.Now,
            ChangeReason = string.IsNullOrEmpty(userComment) ? "Створено новий запит" : $"Коментар: {userComment}"
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin") return Forbid();

        var booking = await _context.Bookings.Include(b => b.Workspace).FirstOrDefaultAsync(b => b.Id == id);
        if (booking == null) return NotFound();

        bool isBusy = await _context.Bookings.AnyAsync(b => 
            b.WorkspaceId == booking.WorkspaceId && b.Id != booking.Id &&
            b.StatusId == 2 &&
            ((booking.StartTime < b.EndTime) && (booking.EndTime > b.StartTime)));

        if (isBusy)
        {
            TempData["Error"] = "Місце вже зайняте іншим підтвердженим бронюванням.";
            return RedirectToAction(nameof(Index));
        }

        if (!await _context.BookingStatuses.AnyAsync(s => s.Id == 2))
        {
            TempData["Error"] = "Статус 'Підтверджено' не знайдено. Зверніться до технічної підтримки.";
            return RedirectToAction(nameof(Index));
        }

        booking.StatusId = 2;
        booking.UpdatedAt = DateTime.Now;

        _context.BookingHistories.Add(new BookingHistory {
            BookingId = booking.Id,
            StatusToId = 2,
            ChangedByUserId = HttpContext.Session.GetInt32("UserId"),
            ChangedAt = DateTime.Now,
            ChangeReason = "Адмін підтвердив бронювання"
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string reason)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();

        string role = HttpContext.Session.GetString("UserRole") ?? "User";
        int? currentUserId = HttpContext.Session.GetInt32("UserId");

        if (role != "Admin" && booking.UserId != currentUserId) return Forbid();

        if (!await _context.BookingStatuses.AnyAsync(s => s.Id == 3))
        {
            TempData["Error"] = "Статус 'Скасовано' не знайдено. Зверніться до технічної підтримки.";
            return RedirectToAction(nameof(Index));
        }

        booking.StatusId = 3;
        booking.UpdatedAt = DateTime.Now;

        _context.BookingHistories.Add(new BookingHistory {
            BookingId = booking.Id,
            StatusToId = 3,
            ChangedByUserId = currentUserId,
            ChangedAt = DateTime.Now,
            ChangeReason = !string.IsNullOrWhiteSpace(reason) ? reason : (role == "Admin" ? "Скасовано адміністратором" : "Скасовано користувачем")
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var booking = await _context.Bookings
            .Include(b => b.Workspace).ThenInclude(w => w.Type)
            .Include(b => b.Status)
            .Include(b => b.User)
            .Include(b => b.BookingHistories.OrderByDescending(h => h.ChangedAt)).ThenInclude(h => h.ChangedByUser)
            .Include(b => b.BookingHistories).ThenInclude(h => h.StatusTo)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (booking == null) return NotFound();

        string role = HttpContext.Session.GetString("UserRole") ?? "User";
        int? currentUserId = HttpContext.Session.GetInt32("UserId");

        if (role != "Admin" && booking.UserId != currentUserId) return Forbid();

        return View(booking);
    }
}