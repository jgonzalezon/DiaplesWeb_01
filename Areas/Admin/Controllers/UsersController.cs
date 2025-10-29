using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace DiaplesWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _um;
        private readonly RoleManager<IdentityRole> _rm;
        public UsersController(UserManager<IdentityUser> um, RoleManager<IdentityRole> rm)
        { _um = um; _rm = rm; }

        public async Task<IActionResult> Index()
            => PartialOrView(await BuildModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string userName, string email, string password, string role = "User")
        {
            if (string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Todos los campos son obligatorios.");
                Response.Headers["X-Partial-Error"] = "1";
                return PartialOrView(await BuildModel());
            }

            var user = new IdentityUser { UserName = userName, Email = email, EmailConfirmed = true };
            var res = await _um.CreateAsync(user, password);

            if (!res.Succeeded)
            {
                foreach (var err in res.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);

                Response.Headers["X-Partial-Error"] = "1";
                return PartialOrView(await BuildModel());
            }

            if (!await _rm.RoleExistsAsync(role))
                await _rm.CreateAsync(new IdentityRole(role));
            await _um.AddToRoleAsync(user, role);

            return PartialOrView(await BuildModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var currentUserId = _um.GetUserId(User);
            if (id == currentUserId)
            {
                ModelState.AddModelError("", "No puedes borrar tu propio usuario mientras estás activo.");
                Response.Headers["X-Partial-Error"] = "1";
                return PartialOrView(await BuildModel());
            }

            var u = await _um.FindByIdAsync(id);
            if (u != null) await _um.DeleteAsync(u);
            return PartialOrView(await BuildModel());
        }

        // Helpers
        private async Task<List<(IdentityUser User, IList<string> Roles)>> BuildModel()
        {
            var users = _um.Users.ToList();
            var model = new List<(IdentityUser User, IList<string> Roles)>();
            foreach (var u in users) model.Add((u, await _um.GetRolesAsync(u)));
            return model;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(string id, string role)
        {
            var currentUserId = _um.GetUserId(User);
            if (id == currentUserId)
            {
                ModelState.AddModelError("", "No puedes modificar tu propio rol mientras estás activo.");
                Response.Headers["X-Partial-Error"] = "1";
                return PartialOrView(await BuildModel());
            }

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(role))
            {
                ModelState.AddModelError("", "Datos inválidos.");
                Response.Headers["X-Partial-Error"] = "1";
                return PartialOrView(await BuildModel());
            }

            var user = await _um.FindByIdAsync(id);
            if (user == null)
            {
                ModelState.AddModelError("", "Usuario no encontrado.");
                Response.Headers["X-Partial-Error"] = "1";
                return PartialOrView(await BuildModel());
            }

            if (!await _rm.RoleExistsAsync(role))
                await _rm.CreateAsync(new IdentityRole(role));

            var currentRoles = await _um.GetRolesAsync(user);
            if (!currentRoles.Contains(role))
            {
                if (currentRoles.Count > 0)
                    await _um.RemoveFromRolesAsync(user, currentRoles);

                await _um.AddToRoleAsync(user, role);
            }

            return PartialOrView(await BuildModel());
        }

        private IActionResult PartialOrView(List<(IdentityUser User, IList<string> Roles)> model)
            => Request.Headers["X-Requested-With"] == "XMLHttpRequest"
               ? PartialView("_UsersTable", model)
               : View(model);
    }
    
    
}
