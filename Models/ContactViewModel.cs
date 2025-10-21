using System.ComponentModel.DataAnnotations;

namespace DiaplesWeb.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Indícanos tu nombre")]        
        [Display(Name = "Nombre")]
        [MaxLength(160)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Necesitamos un email de contacto")]
        [EmailAddress]
        [MaxLength(160)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [MaxLength(30)]
        [Display(Name = "Teléfono")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Cuéntanos en qué podemos ayudarte")]
        [Display(Name = "Mensaje")]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; } = string.Empty;
    }
}
