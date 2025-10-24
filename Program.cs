using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DiaplesWeb.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB + Identity (ruta absoluta a app.db)
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "app.db");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Nota: AddDefaultIdentity + AddRoles es correcto si quieres roles con la UI por defecto.
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

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// 👇 Swagger en DESARROLLO (antes lo tenías en producción)
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
app.MapRazorPages(); // publica /Identity/...
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers(); // API

// Migración + seed + LOG de la ruta de BD
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    app.Logger.LogInformation("SQLite DB path in use: {DbPath}", dbPath);
    await SeedAdminAsync(scope.ServiceProvider);
}

app.Run();

static async Task SeedAdminAsync(IServiceProvider services)
{
    var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = services.GetRequiredService<UserManager<IdentityUser>>();

    const string adminRole = "Admin";
    const string adminEmail = "admin@local.test";
    const string adminPass = "Admin123!";

    if (!await roleMgr.RoleExistsAsync(adminRole))
        await roleMgr.CreateAsync(new IdentityRole(adminRole));

    var user = await userMgr.FindByEmailAsync(adminEmail);
    if (user == null)
    {
        user = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var res = await userMgr.CreateAsync(user, adminPass);
        if (res.Succeeded)
            await userMgr.AddToRoleAsync(user, adminRole);
    }
}
