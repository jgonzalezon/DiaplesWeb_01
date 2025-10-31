// Application layer (por ejemplo en /Services/Contracts)
using DiaplesWeb.Models;

public interface IAttendanceService
{
    Task<Dictionary<int, AttendanceStatus>> GetStatusesAsync(string userId);
    Task<AttendanceStatus> GetStatusAsync(int eventId, string userId);
    Task UpdateAsync(int eventId, string userId, AttendanceStatus status);
}

public record CalendarItemDto(
    int Id, string Title, DateTime Date, string? Location,
    AttendanceStatus Status, string[] ClassNames, object ExtendedProps);

public interface IEventQueryService
{
    Task<List<EventItem>> GetAllOrderedAsync();
    Task<EventItem?> FindAsync(int id);
    Task<List<CalendarItemDto>> GetCalendarAsync(DateTime from, DateTime to, string userId, Func<int,string> linkBuilder);
}
