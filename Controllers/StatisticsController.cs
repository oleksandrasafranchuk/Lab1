using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab1_Project.Models;
using ClosedXML.Excel; 
using Xceed.Words.NET;
using Xceed.Document.NET;


namespace Lab1_Project.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly BookingSystemContext _context;

        public StatisticsController(BookingSystemContext context)
        {
            _context = context;
        }
        private IQueryable<Booking> GetFilteredBookings(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Bookings
                .Include(b => b.Workspace)
                .ThenInclude(w => w.Type)
                .Include(b => b.User)
                .Include(b => b.Status)
                .Where(b => b.Status.StatusName == "Підтверджено");

            if (startDate.HasValue)
                query = query.Where(b => b.StartTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(b => b.EndTime <= endDate.Value);

            return query;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin") return Forbid();

            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
    {
        TempData["Error"] = "Початкова дата не може бути більшою за кінцеву.";
        return View(new StatisticsViewModel { StartDate = startDate, EndDate = endDate });
    }

            var bookings = await GetFilteredBookings(startDate, endDate).ToListAsync();

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

        public async Task<IActionResult> ExportToExcel(DateTime? startDate, DateTime? endDate)
        {
            var bookings = await GetFilteredBookings(startDate, endDate).ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Звіт по доходах");
                worksheet.Cell(1, 1).Value = "№ Кабінету";
                worksheet.Cell(1, 2).Value = "Клієнт";
                worksheet.Cell(1, 3).Value = "Сума (₴)";
                worksheet.Cell(1, 4).Value = "Дата";

                for (int i = 0; i < bookings.Count; i++)
                {
                    worksheet.Cell(i + 2, 1).Value = bookings[i].Workspace.Number;
                    worksheet.Cell(i + 2, 2).Value = bookings[i].User.FullName;
                    worksheet.Cell(i + 2, 3).Value = bookings[i].TotalAmount;
                    worksheet.Cell(i + 2, 4).Value = bookings[i].StartTime.ToString("dd.MM.yyyy");
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var fileName = $"Report_Excel_{DateTime.Now:yyyyMMdd}.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        public async Task<IActionResult> ExportToWord(DateTime? startDate, DateTime? endDate)
        {
            var bookings = await GetFilteredBookings(startDate, endDate).ToListAsync();

            using (var stream = new MemoryStream())
            {
                using (var doc = DocX.Create(stream))
                {
                    doc.InsertParagraph("Звіт по бронюваннях").FontSize(20).Bold().Alignment = Alignment.center;
                    
                    string period = (startDate.HasValue || endDate.HasValue) 
                        ? $"Період: {(startDate?.ToString("dd.MM.yyyy") ?? "...") } — {(endDate?.ToString("dd.MM.yyyy") ?? "...")}"
                        : "За весь час";
                    
                    doc.InsertParagraph(period).FontSize(12).Italic().Alignment = Alignment.center;
                    doc.InsertParagraph($"Дата формування: {DateTime.Now:dd.MM.yyyy}\n");

                    var table = doc.AddTable(bookings.Count + 1, 3);
                    table.Design = Xceed.Document.NET.TableDesign.TableGrid;

                    table.Rows[0].Cells[0].Paragraphs[0].Append("Кабінет");
                    table.Rows[0].Cells[1].Paragraphs[0].Append("Дата");
                    table.Rows[0].Cells[2].Paragraphs[0].Append("Сума");

                    for (int i = 0; i < bookings.Count; i++)
                    {
                        table.Rows[i + 1].Cells[0].Paragraphs[0].Append(bookings[i].Workspace.Number);
                        table.Rows[i + 1].Cells[1].Paragraphs[0].Append(bookings[i].StartTime.ToShortDateString());
                        table.Rows[i + 1].Cells[2].Paragraphs[0].Append(bookings[i].TotalAmount.ToString() + " ₴");
                    }

                    doc.InsertTable(table);
                    doc.SaveAs(stream);
                    var fileName = $"Report_Word_{DateTime.Now:yyyyMMdd}.docx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
                }
            }
        }
    }
}