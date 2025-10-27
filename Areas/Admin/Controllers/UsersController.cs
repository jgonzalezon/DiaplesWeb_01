using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DiaplesWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles="Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _um;
        private readonly RoleManager<IdentityRole> _rm;
        public UsersController(UserManager<IdentityUser> um, RoleManager<IdentityRole> rm)
        { _um = um; _rm = rm; }

        public async Task<IActionResult> Index()
        {
            var users = _um.Users.ToList();
            var model = new List<(IdentityUser User, IList<string> Roles)>();
            foreach (var u in users)
                model.Add((u, await _um.GetRolesAsync(u)));
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string userName, string email, string password, string role = "User")
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return RedirectToAction(nameof(Index));

            var user = new IdentityUser{ UserName = userName, Email = email, EmailConfirmed = true };
            var res = await _um.CreateAsync(user, password);
            if (res.Succeeded)
            {
                if (!await _rm.RoleExistsAsync(role)) await _rm.CreateAsync(new IdentityRole(role));
                await _um.AddToRoleAsync(user, role);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var u = await _um.FindByIdAsync(id);
            if (u != null) await _um.DeleteAsync(u);
            return RedirectToAction(nameof(Index));
        }
    }
}
