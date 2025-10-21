using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DiaplesWeb.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB + Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db"));

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

// ✅ Swagger siempre habilitado (o muévelo al if que prefieras)
app.UseSwagger();
app.UseSwaggerUI(); // /swagger

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 👇 Mantén MVC (vistas)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 👇 IMPORTANTE: publica los controladores API
app.MapControllers();

app.MapRazorPages();

// Migración + seed ...
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
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
