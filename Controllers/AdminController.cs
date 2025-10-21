using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiaplesWeb.Data;
using DiaplesWeb.Models;

namespace DiaplesWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AdminController(ApplicationDbContext db) => _db = db;

        // /Admin/Registrations?page=1&pageSize=20&q=texto
        public async Task<IActionResult> Registrations(int page = 1, int pageSize = 20, string? q = null)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 200);

            var query = _db.Registrations.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim().ToLower();
                query = query.Where(x =>
                    x.Name.ToLower().Contains(s) ||
                    x.Email.ToLower().Contains(s)
                ).OrderByDescending(x => x.CreatedAtUtc);
            }

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var vm = new RegistrationsVm
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                Total = total,
                TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)),
                Query = q ?? string.Empty
            };

            return View(vm);
        }
    }

    public class RegistrationsVm
    {
        public List<Registration> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public int TotalPages { get; set; }
        public string Query { get; set; } = "";
    }
}
