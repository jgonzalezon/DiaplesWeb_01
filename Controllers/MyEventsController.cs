using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DiaplesWeb.Models;

[Authorize]
public class MyEventsController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEventQueryService _events;
    private readonly IAttendanceService _attendance;

    public MyEventsController(
        UserManager<IdentityUser> userManager,
        IEventQueryService events,
        IAttendanceService attendance)
    {
        _userManager = userManager;
        _events = events;
        _attendance = attendance;
    }

    // GET: /MyEvents
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return Challenge();

        var events = await _events.GetAllOrderedAsync();
        ViewBag.MyStatuses = await _attendance.GetStatusesAsync(userId);
        return View(events);
    }

    // POST: /MyEvents/UpdateMyAttendance
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateMyAttendance(int id, AttendanceStatus status)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return Challenge();

        await _attendance.UpdateAsync(id, userId, status);
        TempData["ok"] = "Tu asistencia se ha actualizado.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /MyEvents/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return Challenge();

        var ev = await _events.FindAsync(id);
        if (ev is null) return NotFound();

        var myStatus = await _attendance.GetStatusAsync(id, userId);
        var vm = new { Event = ev, MyStatus = myStatus };
        return View(vm);
    }

    // GET: /MyEvents/CalendarFeed
    [HttpGet]
    public async Task<IActionResult> CalendarFeed([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return Unauthorized();

        var from = start?.Date ?? DateTime.UtcNow.AddMonths(-1);
        var to   = end?.Date   ?? DateTime.UtcNow.AddMonths(2);

        // El service no conoce MVC; le pasamos cómo construir enlaces
        List<CalendarItemDto> items = await _events.GetCalendarAsync(
            from, to, userId,
            linkBuilder: (eventId) => Url.Action("Index", "MyEvents") + "#event-" + eventId
        );

        // Shaping final a lo que espera tu FullCalendar (opcional)
        var shaped = items.Select(i => new {
            id = i.Id,
            title = i.Title,
            start = i.Date.ToString("o"),
            url = (i.ExtendedProps as dynamic).url,
            extendedProps = new { location = i.Location, status = i.Status.ToString() },
            classNames = i.ClassNames
        });

        return Ok(shaped);
    }
}
