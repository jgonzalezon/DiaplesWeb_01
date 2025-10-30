using System.Collections.Generic;
using System.Linq;
using DiaplesWeb.Data;
using DiaplesWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiaplesWeb.Controllers
{
    [Authorize]
    public class UserEventsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public UserEventsController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAttendance(UserEventsUpdateRequest request)
        {
            if (request is null || request.Updates == null || request.Updates.Count == 0)
            {
                return RedirectToLocalOrHome(request?.ReturnUrl);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Forbid();
            }

            var eventIds = request.Updates.Select(u => u.EventId).Distinct().ToList();

            var existingAttendances = await _db.EventAttendances
                .Where(a => a.UserId == user.Id && eventIds.Contains(a.EventItemId))
                .ToListAsync();

            var newAttendances = new List<EventAttendance>();

            foreach (var update in request.Updates)
            {
                var attendance = existingAttendances.FirstOrDefault(a => a.EventItemId == update.EventId);
                if (attendance is null)
                {
                    attendance = new EventAttendance
                    {
                        EventItemId = update.EventId,
                        UserId = user.Id
                    };
                    newAttendances.Add(attendance);
                    existingAttendances.Add(attendance);
                }

                attendance.Status = update.Status;
            }

            if (newAttendances.Count > 0)
            {
                _db.EventAttendances.AddRange(newAttendances);
            }

            await _db.SaveChangesAsync();

            TempData["UserEventsSaved"] = true;

            return RedirectToLocalOrHome(request.ReturnUrl);
        }

        private IActionResult RedirectToLocalOrHome(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
