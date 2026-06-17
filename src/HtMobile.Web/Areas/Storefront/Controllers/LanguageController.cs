using HtMobile.Web.Infrastructure;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Storefront.Controllers;

/// <summary>Đổi ngôn ngữ VI/EN (P3-05): set cookie culture rồi quay lại trang trước (returnUrl nội bộ).</summary>
[Area("Storefront")]
public class LanguageController : Controller
{
    [HttpGet("/changelanguage")]
    public IActionResult Change(string? culture, string? returnUrl)
    {
        var resolved = LanguageOptions.Resolve(culture);
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(resolved)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps
            });

        return Redirect(UrlSafety.SafeLocalUrl(returnUrl));
    }
}
