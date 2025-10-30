using System;
using Microsoft.AspNetCore.Identity;

namespace DiaplesWeb.Models
{
    public class Attendance
    {
        public int EventId { get; set; }
        public string UserId { get; set; } = default!;

        public AttendanceStatus Status { get; set; } = AttendanceStatus.No;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navs
        public EventItem Event { get; set; } = default!;
        public IdentityUser User { get; set; } = default!;
    }
}
