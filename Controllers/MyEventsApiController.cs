using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DiaplesWeb.Models;
using DiaplesWeb.Services.Contracts;

namespace DiaplesWeb.Controllers;

/// <summary>
/// Endpoints JSON equivalentes a /MyEvents para usar desde clientes o Swagger.
/// </summary>
[Authorize]
[ApiController]                         // ← necesario para Swagger y binding/validación automáticos
[Route("api/myevents")]                 // base URL para este recurso
public class MyEventsApiController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEventQueryService _events;
    private readonly IAttendanceService _attendance;

    public MyEventsApiController(
        UserManager<IdentityUser> userManager,
        IEventQueryService events,
        IAttendanceService attendance)
    {
        _userManager = userManager;
        _events = events;
        _attendance = attendance;
    }

    /// <summary>Devuelve eventos paginados (solo futuros), incluyendo tus estados.</summary>
    /// <param name="page">Página (1..n)</param>
    /// <param name="pageSize">Tamaño de página (por defecto 4)</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 4)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return Challenge();

        var (items, total) = await _events.GetPagedAsync(page, pageSize);
        var myStatuses = await _attendance.GetStatusesAsync(userId);

        return Ok(new
        {
            page,
            pageSize,
            total,
            items,
            myStatuses    // diccionario EventId -> AttendanceStatus
        });
    }

    /// <summary>Detalles de un evento + tu estado actual.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetails([FromRoute] int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return Challenge();

        var ev = await _events.FindAsync(id);
        if (ev is null) return NotFound();

        var myStatus = await _attendance.GetStatusAsync(id, userId);
        return Ok(new { ev, myStatus });
    }

    public record UpdateAttendanceDto(AttendanceStatus Status);

    /// <summary>Actualiza tu asistencia a un evento.</summary>
    [HttpPost("{id:int}/attendance")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateAttendance([FromRoute] int id, [FromBody] UpdateAttendanceDto body)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return Challenge();

        await _attendance.UpdateAsync(id, userId, body.Status);
        return NoContent();
    }

    /// <summary>Feed para FullCalendar en el rango indicado.</summary>
    [HttpGet("calendar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CalendarFeed([FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null) return Unauthorized();

        var from = start?.Date ?? DateTime.UtcNow.AddMonths(-1);
        var to   = end?.Date   ?? DateTime.UtcNow.AddMonths(2);

        var items = await _events.GetCalendarAsync(
            from, to, userId,
            linkBuilder: (eventId) => Url.Action("Index", "MyEvents") + "#event-" + eventId
        );

        var shaped = items.Select(i => new
        {
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
