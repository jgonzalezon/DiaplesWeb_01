using System.Linq;
using DiaplesWeb.Data;
using DiaplesWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiaplesWeb.ViewComponents
{
    public class UserEventsPanelViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public UserEventsPanelViewComponent(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                return Content(string.Empty);
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user is null)
            {
                return Content(string.Empty);
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Content(string.Empty);
            }

            var eventData = await _db.Events
                .AsNoTracking()
                .OrderBy(e => e.Date)
                .Select(e => new
                {
                    Event = e,
                    Attendance = e.Attendances
                        .Where(a => a.UserId == user.Id)
                        .Select(a => new { a.Status })
                        .FirstOrDefault()
                })
                .ToListAsync();

            if (eventData.Count == 0)
            {
                return Content(string.Empty);
            }

            var showSavedMessage = TempData.TryGetValue("UserEventsSaved", out _);

            var viewModel = new UserEventsPanelViewModel
            {
                Events = eventData
                    .Select(e => new UserEventAttendanceViewModel
                    {
                        Id = e.Event.Id,
                        Title = e.Event.Title,
                        Date = e.Event.Date,
                        Location = e.Event.Location,
                        Status = e.Attendance?.Status ?? EventAttendanceStatus.No
                    })
                    .ToList(),
                CalendarItems = eventData
                    .Select(e => new EventCalendarItem
                    {
                        EventId = e.Event.Id,
                        Date = e.Event.Date,
                        Title = e.Event.Title,
                        CssClass = GetStatusCssClass(e.Attendance?.Status ?? EventAttendanceStatus.No)
                    })
                    .ToList(),
                ReturnUrl = GetReturnUrl(),
                ShowSavedMessage = showSavedMessage
            };

            return View(viewModel);
        }

        private static string? GetStatusCssClass(EventAttendanceStatus status) => status switch
        {
            EventAttendanceStatus.Yes => "event-calendar__day--yes",
            EventAttendanceStatus.Pending => "event-calendar__day--pending",
            EventAttendanceStatus.No => "event-calendar__day--no",
            _ => null
        };

        private string GetReturnUrl()
        {
            var request = ViewContext.HttpContext.Request;
            var path = request.Path.HasValue ? request.Path.Value : string.Empty;
            var query = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;
            return string.Concat(path, query);
        }
    }
}
