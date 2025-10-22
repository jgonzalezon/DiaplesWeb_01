using System;
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
        [HttpGet]
        public IActionResult Contact()
        {
            var model = new ContactViewModel();
            return View(model);
        }
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var entity = new ContactMessage
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Message = model.Message,
                CreatedAt = DateTime.UtcNow
            };

            _db.ContactMessages.Add(entity);
            _db.SaveChanges();

            TempData["ContactOk"] = "¡Gracias por escribirnos! Te responderemos muy pronto.";
            return RedirectToAction(nameof(Contact));
        }
    }
}
