using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DiaplesWeb.Data;

namespace DiaplesWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles="Admin")]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public MessagesController(ApplicationDbContext db) => _db = db;

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
    }
}
