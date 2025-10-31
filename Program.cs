using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;
using DiaplesWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Servicios ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// BD (SQLite) con ruta absoluta al app.db en la raíz del proyecto
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "app.db");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Identity + Roles
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddScoped<IAttendanceService, EfAttendanceService>();
builder.Services.AddScoped<IEventQueryService, EfEventQueryService>();


var app = builder.Build();

// --- Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// 2️⃣ Luego el default general
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Identity
app.MapControllers(); // API

// --- Migraciones + Seed + Log de ruta de BD ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();

    // Solo migra si hay pendientes
    var pending = await db.Database.GetPendingMigrationsAsync();
    if (pending.Any())
        await db.Database.MigrateAsync();

    app.Logger.LogInformation("SQLite DB path in use: {DbPath}", dbPath);

    // Seed idempotente (solo crea si no existe)
    await SeedAdminAsync(services);
}

app.Run();


// ===================== Helpers =====================
static async Task SeedAdminAsync(IServiceProvider services)
{
    var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = services.GetRequiredService<UserManager<IdentityUser>>();

    const string adminRole = "Admin";
    const string adminEmail = "admin@local.test";
    const string adminPass = "Admin123!";

    // Rol
    if (!await roleMgr.RoleExistsAsync(adminRole))
        await roleMgr.CreateAsync(new IdentityRole(adminRole));

    // Usuario admin (idempotente)
    var user = await userMgr.FindByEmailAsync(adminEmail);
    if (user == null)
    {
        user = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var res = await userMgr.CreateAsync(user, adminPass);
        if (res.Succeeded)
        {
            await userMgr.AddToRoleAsync(user, adminRole);
        }
        else
        {
            // Log simple de errores de creación (opcional)
            var errors = string.Join(" | ", res.Errors.Select(e => $"{e.Code}:{e.Description}"));
            Console.WriteLine($"[SeedAdmin] No se pudo crear el admin: {errors}");
        }
    }
}
