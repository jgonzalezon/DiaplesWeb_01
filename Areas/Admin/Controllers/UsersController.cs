using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;


namespace DiaplesWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _um;
        private readonly RoleManager<IdentityRole> _rm;
        private readonly IStringLocalizer<UsersController> _localizer;

        public UsersController(UserManager<IdentityUser> um, RoleManager<IdentityRole> rm, IStringLocalizer<UsersController> localizer)
        {
            _um = um;
            _rm = rm;
            _localizer = localizer;
        }

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
                ModelState.AddModelError("", _localizer["UsersCreateRequired"].Value);
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
                ModelState.AddModelError("", _localizer["UsersDeleteSelf"].Value);
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
                ModelState.AddModelError("", _localizer["UsersUpdateSelf"].Value);
                Response.Headers["X-Partial-Error"] = "1";
                return PartialOrView(await BuildModel());
            }

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(role))
            {
                ModelState.AddModelError("", _localizer["UsersInvalidData"].Value);
                Response.Headers["X-Partial-Error"] = "1";
                return PartialOrView(await BuildModel());
            }

            var user = await _um.FindByIdAsync(id);
            if (user == null)
            {
                ModelState.AddModelError("", _localizer["UsersNotFound"].Value);
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
