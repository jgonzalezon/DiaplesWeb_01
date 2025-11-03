// Infrastructure layer (por ejemplo en /Services/Ef)
using DiaplesWeb.Data;
using DiaplesWeb.Models;
using DiaplesWeb.Services.Contracts;
using Microsoft.EntityFrameworkCore;


public class EfEventQueryService : IEventQueryService
{
    private readonly ApplicationDbContext _db;
    private readonly IAttendanceService _attendance;
    public EfEventQueryService(ApplicationDbContext db, IAttendanceService attendance)
    {
        _db = db; _attendance = attendance;
    }

public Task<List<EventItem>> GetAllOrderedAsync()
{
    var now = DateTime.Now;
    return _db.Events
        .AsNoTracking()
        .Where(e => e.Date >= now)
        .OrderBy(e => e.Date)
        .ToListAsync();
}

    public Task<EventItem?> FindAsync(int id)
        => _db.Events.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<List<CalendarItemDto>> GetCalendarAsync(
        DateTime from, DateTime to, string userId, Func<int, string> linkBuilder)
    {
        var myStatuses = await _attendance.GetStatusesAsync(userId);

        var items = await _db.Events
            .Where(e => e.Date >= from && e.Date <= to)
            .OrderBy(e => e.Date)
            .Select(e => new { e.Id, e.Title, e.Date, e.Location })
            .ToListAsync();

        return items.Select(e =>
        {
            var status = myStatuses.TryGetValue(e.Id, out var s) ? s : AttendanceStatus.No;
            var cls = status switch
            {
                AttendanceStatus.Yes => "att-yes",
                AttendanceStatus.Maybe => "att-maybe",
                _ => "att-no"
            };
            return new CalendarItemDto(
                e.Id, e.Title, e.Date, e.Location, status,
                new[] { cls },
                new { location = e.Location, status = status.ToString(), url = linkBuilder(e.Id) }
            );
        }).ToList();
    }

public async Task<(List<EventItem> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
{
    if (page < 1) page = 1;
    if (pageSize < 1) pageSize = 4;

    // Solo eventos futuros (o de hoy en adelante)
    var now = DateTime.Now;

    var query = _db.Events
        .AsNoTracking()
        .Where(e => e.Date >= now)       //  excluye pasados
        .OrderBy(e => e.Date);

    var total = await query.CountAsync();
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return (items, total);
}

}