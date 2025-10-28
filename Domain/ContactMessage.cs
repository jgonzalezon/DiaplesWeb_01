using System;
using System.ComponentModel.DataAnnotations;

namespace DiaplesWeb.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required, MaxLength(160)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(160)]
        public string Email { get; set; } = string.Empty;

        [Phone, MaxLength(30)]
        public string? Phone { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
