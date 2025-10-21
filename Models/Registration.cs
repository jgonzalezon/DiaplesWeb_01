using System;
using System.ComponentModel.DataAnnotations;

namespace DiaplesWeb.Models
{
    public class Registration
    {
        public int Id { get; set; }

        [Required, MaxLength(160)]
        public string Name { get; set; } = "";

        [Required, EmailAddress, MaxLength(160)]
        public string Email { get; set; } = "";

        [MaxLength(60)]
        public string Experience { get; set; } = "";

        [MaxLength(60)]
        public string Plan { get; set; } = "";

        [MaxLength(2000)]
        public string? Message { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
