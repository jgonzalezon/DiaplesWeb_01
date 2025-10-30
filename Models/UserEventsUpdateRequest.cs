using System.Collections.Generic;

namespace DiaplesWeb.Models
{
    public class UserEventsUpdateRequest
    {
        public List<UserEventAttendanceUpdate> Updates { get; set; } = new();

        public string? ReturnUrl { get; set; }
    }

    public class UserEventAttendanceUpdate
    {
        public int EventId { get; set; }

        public EventAttendanceStatus Status { get; set; }
    }
}
