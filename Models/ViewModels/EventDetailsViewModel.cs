using System.Collections.Generic;

namespace DiaplesWeb.Models.ViewModels
{
    public class AttendanceRowVM
    {
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public AttendanceStatus Status { get; set; }
    }

    public class EventDetailsViewModel
    {
        public EventItem Event { get; set; } = default!;
        public List<AttendanceRowVM> Rows { get; set; } = new();
    }
}
