using System;
using System.Collections.Generic;

namespace DiaplesWeb.Models
{
    public class AdminEventsIndexViewModel
    {
        public List<AdminEventListItemViewModel> Events { get; set; } = new();

        public int? SelectedEventId { get; set; }

        public AdminEventDetailsViewModel? SelectedEvent { get; set; }

        public IReadOnlyList<EventAttendanceStatusOption> StatusOptions { get; init; } = EventAttendanceStatusExtensions.GetOptions();

        public List<EventCalendarItem> CalendarItems { get; set; } = new();
    }

    public class AdminEventListItemViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string Location { get; set; } = string.Empty;
    }

    public class AdminEventDetailsViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string Location { get; set; } = string.Empty;

        public List<AdminEventAttendeeViewModel> Attendees { get; set; } = new();
    }

    public class AdminEventAttendeeViewModel
    {
        public string DisplayName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public EventAttendanceStatus Status { get; set; }
    }
}
