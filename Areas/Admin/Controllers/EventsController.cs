using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiaplesWeb.Data;
using DiaplesWeb.Models;

namespace DiaplesWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles="Admin")]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public EventsController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? id)
        {
            var events = await _db.Events
                .AsNoTracking()
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            var viewModel = new AdminEventsIndexViewModel
            {
                Events = events
                    .Select(e => new AdminEventListItemViewModel
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Date = e.Date,
                        Location = e.Location
                    })
                    .ToList(),
                CalendarItems = events
                    .Select(e => new EventCalendarItem
                    {
                        EventId = e.Id,
                        Date = e.Date,
                        Title = e.Title
                    })
                    .ToList()
            };

            if (events.Count > 0)
            {
                var selectedId = id ?? events.First().Id;

                var selectedEvent = await _db.Events
                    .Include(e => e.Attendances)
                    .ThenInclude(a => a.User)
                    .FirstOrDefaultAsync(e => e.Id == selectedId);

                if (selectedEvent == null)
                {
                    selectedEvent = await _db.Events
                        .Include(e => e.Attendances)
                        .ThenInclude(a => a.User)
                        .FirstOrDefaultAsync(e => e.Id == events.First().Id);
                    selectedId = selectedEvent?.Id;
                }

                if (selectedEvent != null)
                {
                    viewModel.SelectedEventId = selectedId;
                    viewModel.SelectedEvent = new AdminEventDetailsViewModel
                    {
                        Id = selectedEvent.Id,
                        Title = selectedEvent.Title,
                        Date = selectedEvent.Date,
                        Location = selectedEvent.Location,
                        Attendees = selectedEvent.Attendances
                            .OrderBy(a => a.User.Email ?? a.User.UserName)
                            .Select(a => new AdminEventAttendeeViewModel
                            {
                                DisplayName = !string.IsNullOrWhiteSpace(a.User.UserName)
                                    ? a.User.UserName!
                                    : a.User.Email ?? string.Empty,
                                Email = a.User.Email ?? string.Empty,
                                Status = a.Status
                            })
                            .ToList()
                    };
                }
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create() => View(new EventItem { Date = DateTime.Now });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventItem model)
        {
            if (!ModelState.IsValid) return View(model);

            _db.Events.Add(model);
            await _db.SaveChangesAsync();

            var users = await _userManager.Users.ToListAsync();
            if (users.Count > 0)
            {
                var attendances = users.Select(u => new EventAttendance
                {
                    EventItemId = model.Id,
                    UserId = u.Id,
                    Status = EventAttendanceStatus.No
                }).ToList();

                if (attendances.Count > 0)
                {
                    _db.EventAttendances.AddRange(attendances);
                    await _db.SaveChangesAsync();
                }
            }

            TempData["ok"] = "Evento creado.";
            return RedirectToAction(nameof(Index), new { id = model.Id });
        }
    }
}
