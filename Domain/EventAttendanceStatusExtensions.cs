using System.Collections.Generic;

namespace DiaplesWeb.Models
{
    public static class EventAttendanceStatusExtensions
    {
        private static readonly IReadOnlyList<EventAttendanceStatusOption> _options = new List<EventAttendanceStatusOption>
        {
            new(EventAttendanceStatus.Yes, "Sí"),
            new(EventAttendanceStatus.No, "No"),
            new(EventAttendanceStatus.Pending, "Pendiente")
        };

        public static string ToDisplayName(this EventAttendanceStatus status) => status switch
        {
            EventAttendanceStatus.Yes => "Sí",
            EventAttendanceStatus.No => "No",
            EventAttendanceStatus.Pending => "Pendiente",
            _ => status.ToString()
        };

        public static IReadOnlyList<EventAttendanceStatusOption> GetOptions() => _options;
    }

    public record EventAttendanceStatusOption(EventAttendanceStatus Status, string DisplayName);
}
