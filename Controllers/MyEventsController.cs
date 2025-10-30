using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiaplesWeb.Data;
using DiaplesWeb.Models;
using System.Globalization;

namespace DiaplesWeb.Controllers
{
    [Authorize]  // cualquier usuario autenticado
    public class MyEventsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public MyEventsController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: /MyEvents
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var events = await _db.Events
                .OrderBy(e => e.Date)
                .ToListAsync();

            var myStatuses = await _db.Attendances
                .Where(a => a.UserId == user.Id)
                .ToDictionaryAsync(a => a.EventId, a => a.Status);

            ViewBag.MyStatuses = myStatuses;
            return View(events);
        }

        // POST: /MyEvents/UpdateMyAttendance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMyAttendance(int id, AttendanceStatus status)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var row = await _db.Attendances.FindAsync(id, user.Id);
            if (row == null)
            {
                // Si no existe (por seguridad), se crea
                row = new Attendance { EventId = id, UserId = user.Id };
                _db.Attendances.Add(row);
            }

            row.Status = status;
            row.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["ok"] = "Tu asistencia se ha actualizado.";
            return RedirectToAction(nameof(Index));
        }

        // JSON feed reutilizado para el calendario
        [HttpGet]
        public async Task<IActionResult> CalendarFeed([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var from = start?.Date ?? DateTime.UtcNow.AddMonths(-1);
            var to   = end?.Date ?? DateTime.UtcNow.AddMonths(2);

            var items = await _db.Events
                .Where(e => e.Date >= from && e.Date <= to)
                .OrderBy(e => e.Date)
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    start = e.Date.ToString("o", CultureInfo.InvariantCulture),
                    url = Url.Action("Index", "MyEvents") + "#event-" + e.Id,
                    extendedProps = new { location = e.Location }
                })
                .ToListAsync();

            return Ok(items);
        }
    }
}
