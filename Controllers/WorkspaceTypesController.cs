using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab1_Project.Models;
using ClosedXML.Excel;
using Xceed.Words.NET;

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



[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Import(IFormFile file)
{
    if (HttpContext.Session.GetString("UserRole") != "Admin") return Forbid();

    if (file == null || file.Length == 0)
    {
        TempData["Error"] = "Будь ласка, виберіть файл.";
        return RedirectToAction(nameof(Index));
    }

    var newTypes = new List<WorkspaceType>();
    var extension = Path.GetExtension(file.FileName).ToLower();

    try
    {
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            stream.Position = 0;

            if (extension == ".xlsx") 
            {
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RangeUsed().RowsUsed().Skip(1); 

                foreach (var row in rows)
                {
                    var name = row.Cell(1).GetValue<string>();
                    var desc = row.Cell(2).GetValue<string>();
                    if (!string.IsNullOrEmpty(name)) 
                        newTypes.Add(new WorkspaceType { TypeName = name, Description = desc });
                }
            }
            else if (extension == ".docx") 
            {
                using var document = DocX.Load(stream);
                if (document.Tables.Count > 0)
                {
                    var table = document.Tables[0];
                    foreach (var row in table.Rows.Skip(1)) 
                    {
                        var name = row.Cells[0].Paragraphs[0].Text.Trim();
                        var desc = row.Cells.Count > 1 ? row.Cells[1].Paragraphs[0].Text.Trim() : "";
                        if (!string.IsNullOrEmpty(name))
                            newTypes.Add(new WorkspaceType { TypeName = name, Description = desc });
                    }
                }
            }
            else
            {
                TempData["Error"] = "Формат файлу не підтримується. Використовуйте .xlsx або .docx";
                return RedirectToAction(nameof(Index));
            }
        }

        int addedCount = 0;
        foreach (var type in newTypes)
        {
            if (!await _context.WorkspaceTypes.AnyAsync(t => t.TypeName == type.TypeName))
            {
                _context.WorkspaceTypes.Add(type);
                addedCount++;
            }
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Успішно додано {addedCount} нових типів.";
    }
    catch (Exception ex)
    {
        TempData["Error"] = "Помилка при обробці файлу: " + ex.Message;
    }

    return RedirectToAction(nameof(Index));
}
}