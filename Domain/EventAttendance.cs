using Microsoft.AspNetCore.Identity;

namespace DiaplesWeb.Models
{
    public class EventAttendance
    {
        public int Id { get; set; }

        public int EventItemId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public EventAttendanceStatus Status { get; set; } = EventAttendanceStatus.No;

        public EventItem Event { get; set; } = default!;

        public IdentityUser User { get; set; } = default!;
    }
}
