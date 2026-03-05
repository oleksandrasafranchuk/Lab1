using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab1_Project.Models;

namespace Lab1_Project.Controllers;

public class WorkspacesController : Controller
{
    private readonly BookingSystemContext _context;

    public WorkspacesController(BookingSystemContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? typeId, decimal? minPrice, decimal? maxPrice, string searchNumber, DateTime? searchStart, DateTime? searchEnd)
    {
        string role = HttpContext.Session.GetString("UserRole") ?? "User";
        if (searchStart.HasValue && searchEnd.HasValue)
    {
        if (searchStart >= searchEnd)
        {
            TempData["Error"] = "Час початку оренди не може бути пізнішим або рівним часу завершення.";
            searchStart = null;
            searchEnd = null;
        }
        else if (searchStart < DateTime.Now.AddMinutes(-5))
        {
            TempData["Error"] = "Не можна забронювати місце на минулий час.";
            searchStart = null;
            searchEnd = null;
        }
    }
        var query = _context.Workspaces
            .Include(w => w.Type)
            .AsQueryable();

        if (role != "Admin")
        {
            query = query.Where(w => w.IsActive);
        }

        if (!string.IsNullOrEmpty(searchNumber))
            query = query.Where(w => w.Number.Contains(searchNumber));

        if (typeId.HasValue) 
            query = query.Where(w => w.TypeId == typeId);

        if (minPrice.HasValue) 
            query = query.Where(w => w.PricePerHour >= minPrice.Value);

        if (maxPrice.HasValue) 
            query = query.Where(w => w.PricePerHour <= maxPrice.Value);

        if (searchStart.HasValue && searchEnd.HasValue)
        {
            var busyIds = await _context.Bookings
                .Where(b => b.Status.StatusName != "Скасовано")
                .Where(b => (searchStart < b.EndTime) && (searchEnd > b.StartTime))
                .Select(b => b.WorkspaceId)
                .Distinct()
                .ToListAsync();

            query = query.Where(w => !busyIds.Contains(w.Id));
        }

        ViewBag.TypeId = new SelectList(_context.WorkspaceTypes, "Id", "TypeName");
        ViewBag.CurrentStart = searchStart?.ToString("yyyy-MM-ddTHH:mm");
        ViewBag.CurrentEnd = searchEnd?.ToString("yyyy-MM-ddTHH:mm");

        return View(await query.ToListAsync());
    }

   public async Task<IActionResult> Details(int? id)
{
    if (id == null) return NotFound();
    var workspace = await _context.Workspaces
        .Include(w => w.Type)
        .FirstOrDefaultAsync(m => m.Id == id);

    if (workspace == null) return NotFound();

    return View(workspace);
}

    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin") return Forbid();
        ViewBag.TypeId = new SelectList(_context.WorkspaceTypes, "Id", "TypeName");
        return View();
    }

    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Workspace workspace)
{
    if (HttpContext.Session.GetString("UserRole") != "Admin") return Forbid();

    ModelState.Remove("Type");
    ModelState.Remove("Bookings");

    if (ModelState.IsValid)
    {
        workspace.CreatedAt = DateTime.Now;
        workspace.IsActive = true; 
        _context.Add(workspace);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    ViewBag.TypeId = new SelectList(_context.WorkspaceTypes, "Id", "TypeName", workspace.TypeId);
    return View(workspace);
}
    public async Task<IActionResult> Edit(int? id)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin") return Forbid();
        var workspace = await _context.Workspaces.FindAsync(id);
        if (workspace == null) return NotFound();

        ViewBag.TypeId = new SelectList(_context.WorkspaceTypes, "Id", "TypeName", workspace.TypeId);
        return View(workspace);
    }

    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Workspace workspace)
{
    if (id != workspace.Id || HttpContext.Session.GetString("UserRole") != "Admin") 
        return Forbid();

    // Видаляємо перевірку властивостей, які не заповнюються у формі
    ModelState.Remove("Type");
    ModelState.Remove("Bookings");

    if (ModelState.IsValid)
    {
        try
        {
            workspace.UpdatedAt = DateTime.Now;
            _context.Update(workspace);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Workspaces.Any(e => e.Id == workspace.Id)) return NotFound();
            else throw;
        }
        return RedirectToAction(nameof(Index));
    }
    
    ViewBag.TypeId = new SelectList(_context.WorkspaceTypes, "Id", "TypeName", workspace.TypeId);
    return View(workspace);
}

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin") return Forbid();

        var workspace = await _context.Workspaces.FindAsync(id);
        if (workspace != null)
        {
            workspace.IsActive = false;
            workspace.UpdatedAt = DateTime.Now;
            _context.Update(workspace);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    
}