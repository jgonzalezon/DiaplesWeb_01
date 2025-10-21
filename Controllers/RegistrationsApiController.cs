using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiaplesWeb.Data;
using DiaplesWeb.Models;

namespace DiaplesWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationsApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public RegistrationsApiController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    [Authorize(Roles = "Admin")] // protegido con Identity/roles
    public async Task<object> Get(int page = 1, int pageSize = 20)
    {
        var q = _db.Registrations.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc);
        var total = await q.CountAsync();
        var items = await q.Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
        return new { total, page, pageSize, items };
    }

    [HttpPost]
    [AllowAnonymous] // cámbialo si quieres
    public async Task<ActionResult<Registration>> Post(Registration dto)
    {
        dto.CreatedAtUtc = DateTime.UtcNow;
        _db.Registrations.Add(dto);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }
}
