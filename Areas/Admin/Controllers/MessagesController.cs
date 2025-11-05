using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DiaplesWeb.Data;
using DiaplesWeb.Models;
using Microsoft.Extensions.Localization;


namespace DiaplesWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles="Admin")]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IStringLocalizer<MessagesController> _localizer;

        public MessagesController(ApplicationDbContext db, IStringLocalizer<MessagesController> localizer)
        {
            _db = db;
            _localizer = localizer;
        }

        public IActionResult Index()
        {
            var list = _db.ContactMessages
                          .OrderByDescending(x => x.CreatedAt)
                          .ToList();
            return View(list);
        }

        public IActionResult Details(int id)
        {
            var m = _db.ContactMessages.Find(id);
            if (m == null) return NotFound();
            return View(m);
        }

        // NUEVO: borrar desde Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var m = _db.ContactMessages.Find(id);
            if (m == null)
            {
                TempData["MsgError"] = _localizer["MessageMissing"].Value;
                return RedirectToAction(nameof(Index));
            }

            _db.ContactMessages.Remove(m);
            _db.SaveChanges();

            TempData["MsgOk"] = _localizer["MessageDeleted"].Value;
            return RedirectToAction(nameof(Index));
        }
    }
}
