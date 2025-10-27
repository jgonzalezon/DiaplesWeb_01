using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DiaplesWeb.Data;
using DiaplesWeb.Models;

namespace DiaplesWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles="Admin")]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public EventsController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public IActionResult Create() => View(new EventItem{ Date = DateTime.Now });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EventItem model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.Events.Add(model);
            _db.SaveChanges();
            TempData["ok"] = "Evento creado.";
            return RedirectToAction(nameof(Create));
        }
    }
}
