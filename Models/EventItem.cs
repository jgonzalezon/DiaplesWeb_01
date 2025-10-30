using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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

        // (añadiremos después la navegación)
        public ICollection<Attendance>? Attendances { get; set; }
    }
}
