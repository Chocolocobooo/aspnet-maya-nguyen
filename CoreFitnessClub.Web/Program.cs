using CoreFitnessClub.Application.Interfaces;
using CoreFitnessClub.Application.Services;
using CoreFitnessClub.Domain.Entities;
using CoreFitnessClub.Domain.Interfaces;
using CoreFitnessClub.Infrastructure.Data;
using CoreFitnessClub.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── ASP.NET Identity ──────────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit           = true;
    options.Password.RequireLowercase       = true;
    options.Password.RequireUppercase       = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength         = 8;
    options.User.RequireUniqueEmail         = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath        = "/Account/Login";
    options.LogoutPath       = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});

// ── Repositories (Repository Pattern) ────────────────────────────────────────
builder.Services.AddScoped<IMembershipPlanRepository, MembershipPlanRepository>();
builder.Services.AddScoped<IUserMembershipRepository, UserMembershipRepository>();
builder.Services.AddScoped<IFitnessClassRepository,  FitnessClassRepository>();
builder.Services.AddScoped<IBookingRepository,        BookingRepository>();

// ── Application Services (Service Pattern) ────────────────────────────────────
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<IBookingService,    BookingService>();
builder.Services.AddScoped<IUserService,       UserService>();

// ── MVC ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────────
app.UseStatusCodePagesWithReExecute("/Home/Error404");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ── Auto-apply migrations on startup ─────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
