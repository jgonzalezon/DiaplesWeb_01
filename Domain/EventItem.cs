using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiaplesWeb.Models
{
    public class EventItem
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }   // día + hora

        [Required, StringLength(160)]
        public string Location { get; set; } = string.Empty;

        public ICollection<EventAttendance> Attendances { get; set; } = new List<EventAttendance>();
    }
}
