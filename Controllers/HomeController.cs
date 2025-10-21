using Microsoft.AspNetCore.Mvc;
using DiaplesWeb.Models;
using DiaplesWeb.Data;

namespace DiaplesWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index() => View();
        public IActionResult About() => View();
        public IActionResult Contact() => View();
        public IActionResult Espectaculos() => View();
        public IActionResult Galeria() => View();

        [HttpGet]
        public IActionResult Register() => View(new RegistrationViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegistrationViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var reg = new Registration
            {
                Name = model.Name,
                Email = model.Email,
                Experience = model.Experience,
                Plan = model.Plan,
                Message = model.Message
            };

            _db.Registrations.Add(reg);
            _db.SaveChanges();

            TempData["RegisterOk"] = $"¡Gracias {model.Name}! Tu registro se ha guardado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
