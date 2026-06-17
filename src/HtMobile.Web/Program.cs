using HtMobile.Application;
using HtMobile.Application.Common.Interfaces;
using HtMobile.Infrastructure;
using HtMobile.Infrastructure.Persistence.Seed;
using HtMobile.Web.Infrastructure;
using HtMobile.Web.Infrastructure.Routing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddViewLocalization();
// KHÔNG set ResourcesPath: resx được SDK đặt tên theo namespace của SharedResource.cs (DependentUpon)
// = "HtMobile.Web.SharedResource.resources", khớp IStringLocalizer<HtMobile.Web.SharedResource>.
builder.Services.AddLocalization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

// Antiforgery: cho phép htmx gửi token qua header (xem _Layout htmx:configRequest).
builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

// Clean Architecture: nối các lớp tại composition root
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Adapter mức Web cho interface khai báo ở Application
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ISlugResolver, SlugResolver>();
builder.Services.AddScoped<HtMobile.Web.Infrastructure.CartContext>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Đa ngôn ngữ VI/EN (P3-05): culture theo cookie .AspNetCore.Culture, mặc định vi.
app.UseRequestLocalization(new Microsoft.AspNetCore.Builder.RequestLocalizationOptions()
    .SetDefaultCulture(LanguageOptions.Default)
    .AddSupportedCultures(LanguageOptions.Supported)
    .AddSupportedUICultures(LanguageOptions.Supported));

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Storefront + API dùng attribute routing (slug SEO ở gốc). (Admin MVC đã bỏ — admin sẽ là app Angular riêng.)
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
