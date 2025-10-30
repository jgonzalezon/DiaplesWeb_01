using System;
using System.Collections.Generic;

namespace DiaplesWeb.Models
{
    public class UserEventsPanelViewModel
    {
        public List<UserEventAttendanceViewModel> Events { get; set; } = new();

        public List<EventCalendarItem> CalendarItems { get; set; } = new();

        public IReadOnlyList<EventAttendanceStatusOption> StatusOptions { get; init; } = EventAttendanceStatusExtensions.GetOptions();

        public string ReturnUrl { get; set; } = string.Empty;

        public bool ShowSavedMessage { get; set; }
    }

    public class UserEventAttendanceViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string Location { get; set; } = string.Empty;

        public EventAttendanceStatus Status { get; set; }
    }
}
