using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiaplesWeb.Data;
using DiaplesWeb.Models;
using System.Globalization;
using DiaplesWeb.Models.ViewModels;
using System.Linq;



namespace DiaplesWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public EventsController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ====== NUEVO: Lista izquierda + Calendario derecha ======
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Lista de eventos (próximos primero), muestra también los pasados ordenados por fecha descendente al final
            var events = await _db.Events
                .OrderBy(e => e.Date >= DateTime.Now ? 0 : 1) // primero próximos (0), luego pasados (1)
                .ThenBy(e => e.Date)
                .ToListAsync();

            return View(events);
        }

        // ====== NUEVO: Feed JSON para FullCalendar ======
        // FullCalendar envía normalmente ?start=YYYY-MM-DD&end=YYYY-MM-DD (rango visible)
        [HttpGet]
        public async Task<IActionResult> CalendarFeed([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            // Rango de seguridad por si no llega start/end
            var from = start?.Date ?? DateTime.UtcNow.Date.AddMonths(-1);
            var to   = end?.Date   ?? DateTime.UtcNow.Date.AddMonths(2);

            // Filtra por rango visible (mejor rendimiento)
            var items = await _db.Events
                .Where(e => e.Date >= from && e.Date <= to)
                .OrderBy(e => e.Date)
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    start = e.Date.ToString("o", CultureInfo.InvariantCulture), // ISO 8601
                    url = Url.Action("Details", "Events", new { area = "Admin", id = e.Id }),
                    extendedProps = new
                    {
                        location = e.Location
                    }
                })
                .ToListAsync();

            return Ok(items);
        }

        // ========== Ya tenías Create GET/POST; los mantenemos ==========
        [HttpGet]
        public IActionResult Create() => View(new EventItem { Date = DateTime.Now });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventItem model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Events.Add(model);
            await _db.SaveChangesAsync();

            // Si ya tienes Attendance y sembrado, deja este bloque:
            var users = await _userManager.Users.AsNoTracking().ToListAsync();
            var rows = users.Select(u => new Attendance
            {
                EventId = model.Id,
                UserId = u.Id,
                Status = AttendanceStatus.No,
                UpdatedAt = DateTime.UtcNow
            });
            await _db.Attendances.AddRangeAsync(rows);
            await _db.SaveChangesAsync();

            TempData["ok"] = "Evento creado.";
            return RedirectToAction(nameof(Create));
        }

        [HttpGet]
[HttpGet]
public async Task<IActionResult> Details(int id)
{
    var ev = await _db.Events.FindAsync(id);
    if (ev == null) return NotFound();

    // Traemos todas las asistencias de ese evento
    var attendance = await _db.Attendances
        .Where(a => a.EventId == id)
        .ToListAsync();

    // Todos los usuarios registrados
    var users = await _userManager.Users.AsNoTracking().ToListAsync();

    // Construimos las filas uniendo Users + Attendance (LEFT JOIN manual)
    var rows = users.Select(u =>
    {
        var att = attendance.FirstOrDefault(a => a.UserId == u.Id);
        return new AttendanceRowVM
        {
            UserId = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Status = att?.Status ?? AttendanceStatus.No // si no hay registro, se da por No
        };
    })
    .OrderBy(r => r.Status == AttendanceStatus.Yes ? 0 :
                  r.Status == AttendanceStatus.Maybe ? 1 : 2)
    .ThenBy(r => r.UserName)
    .ToList();

    var vm = new EventDetailsViewModel
    {
        Event = ev,
        Rows = rows
    };

    return View(vm);
}


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAttendance(int id, string userId, AttendanceStatus status)
    {
        // id = EventId
        var row = await _db.Attendances.FindAsync(id, userId);
        if (row == null)
        {
            // Crear el registro si no existía
            row = new Attendance
            {
                EventId = id,
                UserId = userId,
                Status = status,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Attendances.Add(row);
        }
        else
        {
            // Actualizar si ya existe
            row.Status = status;
            row.UpdatedAt = DateTime.UtcNow;
            _db.Attendances.Update(row);
        }

        await _db.SaveChangesAsync();
        TempData["ok"] = "Asistencia actualizada.";
        return RedirectToAction(nameof(Details), new { id });
    }
    }
}
