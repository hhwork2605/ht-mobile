using HtMobile.Application;
using HtMobile.Application.Common.Interfaces;
using HtMobile.Infrastructure;
using HtMobile.Infrastructure.Persistence.Seed;
using HtMobile.Web.Infrastructure;
using HtMobile.Web.Infrastructure.Routing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

// Clean Architecture: nối các lớp tại composition root
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Adapter mức Web cho interface khai báo ở Application
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ICurrentRegion, CurrentRegion>();
builder.Services.AddScoped<ISlugResolver, SlugResolver>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Admin area (conventional): /admin/{controller}/{action}
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Storefront + API dùng attribute routing (slug SEO ở gốc)
app.MapControllers();

// Dev: tự apply migration + seed (bỏ qua nếu DB chưa cấu hình)
if (app.Environment.IsDevelopment())
{
    try
    {
        await DbInitializer.RunAsync(app.Services);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex,
            "Bỏ qua migrate/seed — kiểm tra ConnectionStrings (user-secrets) và Redis. Xem docs/setup.md");
    }
}

app.Run();

public partial class Program { }
