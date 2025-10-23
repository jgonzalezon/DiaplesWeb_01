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
