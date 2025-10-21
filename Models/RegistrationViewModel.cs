using System.ComponentModel.DataAnnotations;

namespace DiaplesWeb.Models // <-- ajusta el namespace si aplica
{
    public class RegistrationViewModel
    {
        [Required, Display(Name = "Nombre completo")]
        public string Name { get; set; } = "";

        [Required, EmailAddress, Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Display(Name = "Experiencia")]
        public string Experience { get; set; } = "Principiante";

        [Display(Name = "Plan")]
        public string Plan { get; set; } = "On-Demand";

        [Display(Name = "Mensaje (opcional)")]
        public string? Message { get; set; }
    }
}
