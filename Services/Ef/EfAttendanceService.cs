// Infrastructure layer (por ejemplo en /Services/Ef)
using DiaplesWeb.Data;
using DiaplesWeb.Models;
using Microsoft.EntityFrameworkCore;

public class EfAttendanceService : IAttendanceService
{
    private readonly ApplicationDbContext _db;
    public EfAttendanceService(ApplicationDbContext db) => _db = db;

    public async Task<Dictionary<int, AttendanceStatus>> GetStatusesAsync(string userId)
        => await _db.Attendances
            .Where(a => a.UserId == userId)
            .ToDictionaryAsync(a => a.EventId, a => a.Status);

    public async Task<AttendanceStatus> GetStatusAsync(int eventId, string userId)
    {
        var row = await _db.Attendances.FindAsync(eventId, userId);
        return row?.Status ?? AttendanceStatus.No;
    }

    public async Task UpdateAsync(int eventId, string userId, AttendanceStatus status)
    {
        var row = await _db.Attendances.FindAsync(eventId, userId);
        if (row == null)
        {
            row = new Attendance { EventId = eventId, UserId = userId };
            _db.Attendances.Add(row);
        }
        row.Status = status;
        row.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}