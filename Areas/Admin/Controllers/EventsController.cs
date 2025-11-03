using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiaplesWeb.Data;
using DiaplesWeb.Models;
using DiaplesWeb.Models.ViewModels;
using DiaplesWeb.Services.Contracts;   // 👈 usa los servicios
using System.Globalization;

namespace DiaplesWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEventQueryService _events;   // 👈 consultas de eventos (paginado, calendario)

        public EventsController(
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            IEventQueryService events)
        {
            _db = db;
            _userManager = userManager;
            _events = events;
        }

        // ====== LISTA + PAGINACIÓN (4 por página) ======
        // GET: /Admin/Events?page=1
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 4;
            // Reutiliza el mismo filtrado que en usuarios (futuros / hoy en adelante)
            var (items, total) = await _events.GetPagedAsync(page, pageSize);

            var vm = new PagedEventsViewModel
            {
                Events = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
            return View(vm);
        }

        // ====== FEED JSON PARA FULLCALENDAR ======
        [HttpGet]
        public async Task<IActionResult> CalendarFeed([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var from = start?.Date ?? DateTime.UtcNow.AddMonths(-1);
            var to   = end?.Date   ?? DateTime.UtcNow.AddMonths(2);

            // Para admin no necesitamos colorear por asistencia, pero podemos reutilizar el service:
            var items = await _events.GetCalendarAsync(
                from, to, userId: _userManager.GetUserId(User) ?? string.Empty,
                linkBuilder: (eventId) => Url.Action("Details", "Events", new { area = "Admin", id = eventId })!
            );

            var shaped = items.Select(i => new
            {
                id = i.Id,
                title = i.Title,
                start = i.Date.ToString("o"),
                url = (i.ExtendedProps as dynamic).url,
                extendedProps = new { location = i.Location },
                classNames = i.ClassNames
            });

            return Ok(shaped);
        }

        // ====== CREATE ======
        [HttpGet]
        public IActionResult Create() => View(new EventItem { Date = DateTime.Now });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventItem model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Events.Add(model);
            await _db.SaveChangesAsync();

            // (Opcional) crear asistencia "No" para todos los usuarios
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

        // ====== DETAILS (admin ve todas las asistencias) ======
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();

            var attendance = await _db.Attendances
                .Where(a => a.EventId == id)
                .ToListAsync();

            var users = await _userManager.Users.AsNoTracking().ToListAsync();

            var rows = users.Select(u =>
            {
                var att = attendance.FirstOrDefault(a => a.UserId == u.Id);
                return new AttendanceRowVM
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Status = att?.Status ?? AttendanceStatus.No
                };
            })
            .OrderBy(r => r.Status == AttendanceStatus.Yes ? 0 :
                          r.Status == AttendanceStatus.Maybe ? 1 : 2)
            .ThenBy(r => r.UserName)
            .ToList();

            var vm = new EventDetailsViewModel { Event = ev, Rows = rows };
            return View(vm);
        }

        // ====== UPDATE asistencia de un usuario en este evento ======
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAttendance(int id, string userId, AttendanceStatus status)
        {
            var row = await _db.Attendances.FindAsync(id, userId);
            if (row == null)
            {
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
                row.Status = status;
                row.UpdatedAt = DateTime.UtcNow;
                _db.Attendances.Update(row);
            }

            await _db.SaveChangesAsync();
            TempData["ok"] = "Asistencia actualizada.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null)
            {
                TempData["MsgError"] = "El evento no existe o ya fue borrado.";
                return RedirectToAction(nameof(Index));
            }

            // Eliminar asistencias asociadas (por si no hay cascade)
            var att = await _db.Attendances
                .Where(a => a.EventId == id)
                .ToListAsync();
            if (att.Count > 0)
                _db.Attendances.RemoveRange(att);

            _db.Events.Remove(ev);

            try
            {
                await _db.SaveChangesAsync();
                TempData["ok"] = "Evento borrado correctamente.";
            }
            catch (DbUpdateException)
            {
                TempData["MsgError"] = "No se pudo borrar el evento. Revisa dependencias.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
