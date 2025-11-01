// Application layer (por ejemplo en /Services/Contracts)
using DiaplesWeb.Models;

public interface IAttendanceService
{
    Task<Dictionary<int, AttendanceStatus>> GetStatusesAsync(string userId);
    Task<AttendanceStatus> GetStatusAsync(int eventId, string userId);
    Task UpdateAsync(int eventId, string userId, AttendanceStatus status);
}

