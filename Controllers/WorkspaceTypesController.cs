using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab1_Project.Models;

namespace Lab1_Project.Controllers;

public class WorkspaceTypesController : Controller
{
    private readonly BookingSystemContext _context;
    public WorkspaceTypesController(BookingSystemContext context) => _context = context;

    public async Task<IActionResult> Index() 
{
    
    if (HttpContext.Session.GetString("UserRole") != "Admin") 
    {
        return Forbid();
    }

    var types = await _context.WorkspaceTypes.ToListAsync();
    return View(types);
}

    public IActionResult Create() 
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin") return Forbid();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkspaceType type)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin") return Forbid();

        if (ModelState.IsValid)
        {
            _context.Add(type);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(type);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin") return Forbid();

        var type = await _context.WorkspaceTypes.Include(t => t.Workspaces).FirstOrDefaultAsync(t => t.Id == id);
        
        if (type != null && !type.Workspaces.Any())
        {
            _context.WorkspaceTypes.Remove(type);
            await _context.SaveChangesAsync();
        }
        else
        {
            TempData["Error"] = "Неможливо видалити тип, бо існують робочі місця з цим типом!";
        }
        return RedirectToAction(nameof(Index));
    }

public async Task<IActionResult> Edit(int? id)
{
    if (id == null || HttpContext.Session.GetString("UserRole") != "Admin") return Forbid();

    var workspaceType = await _context.WorkspaceTypes.FindAsync(id);
    if (workspaceType == null) return NotFound();
    
    return View(workspaceType);
}


[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, WorkspaceType workspaceType)
{
    if (id != workspaceType.Id || HttpContext.Session.GetString("UserRole") != "Admin") return Forbid();
    ModelState.Remove("Workspaces");

    if (ModelState.IsValid)
    {
        try
        {
            _context.Update(workspaceType);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.WorkspaceTypes.Any(e => e.Id == workspaceType.Id)) return NotFound();
            else throw;
        }
        return RedirectToAction(nameof(Index));
    }
    return View(workspaceType);
}
}