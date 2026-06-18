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

// Auth storefront = cookie riêng cho tài khoản Customer (tách khỏi Identity — ADR 0004).
// AddInfrastructure đăng ký Identity (cho Admin API) và ĐẶT sẵn DefaultAuthenticate/Challenge/SignIn scheme
// sang cookie Identity → phải ghi đè TƯỜNG MINH cả 3 sang "Storefront" cho app Web (storefront-only).
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = StorefrontAuth.Scheme;
        options.DefaultAuthenticateScheme = StorefrontAuth.Scheme;
        options.DefaultChallengeScheme = StorefrontAuth.Scheme;
        options.DefaultSignInScheme = StorefrontAuth.Scheme;
    })
    .AddCookie(StorefrontAuth.Scheme, options =>
    {
        options.Cookie.Name = "htm_auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    });

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

// Đa ngôn ngữ VI/EN (P3-05): culture theo lựa chọn người dùng (query ?culture / cookie .AspNetCore.Culture),
// mặc định vi. KHÔNG dùng AcceptLanguageHeaderRequestCultureProvider để tránh trình duyệt (Accept-Language: en)
// tự ép sang EN khi khách chưa chọn — site là tiếng Việt trước.
var localizationOptions = new Microsoft.AspNetCore.Builder.RequestLocalizationOptions()
    .SetDefaultCulture(LanguageOptions.Default)
    .AddSupportedCultures(LanguageOptions.Supported)
    .AddSupportedUICultures(LanguageOptions.Supported);
localizationOptions.RequestCultureProviders = localizationOptions.RequestCultureProviders
    .Where(p => p is not Microsoft.AspNetCore.Localization.AcceptLanguageHeaderRequestCultureProvider)
    .ToList();
app.UseRequestLocalization(localizationOptions);

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
