using System;

namespace DiaplesWeb.Models
{
    public class EventCalendarItem
    {
        public int EventId { get; set; }

        public DateTime Date { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? CssClass { get; set; }
    }
}
