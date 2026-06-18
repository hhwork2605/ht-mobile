using System.Security.Claims;
using HtMobile.Domain.Entities.Customers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace HtMobile.Web.Infrastructure;

/// <summary>
/// Đăng nhập storefront bằng cookie riêng (tài khoản <see cref="Customer"/>), TÁCH KHỎI ASP.NET Identity
/// (Identity nay chỉ phục vụ Admin API — ADR 0004). Claim <see cref="ClaimTypes.NameIdentifier"/> = Customer.Id
/// nên <c>CurrentUser</c>/<c>CartContext</c> dùng lại nguyên vẹn.
/// </summary>
public static class StorefrontAuth
{
    public const string Scheme = "Storefront";

    /// <summary>Tạo ClaimsPrincipal cho 1 Customer (id, tên, email).</summary>
    public static ClaimsPrincipal BuildPrincipal(Customer customer)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, customer.Id.ToString()),
            new(ClaimTypes.Name, customer.FullName ?? customer.Email),
            new(ClaimTypes.Email, customer.Email),
        };
        var identity = new ClaimsIdentity(claims, Scheme);
        return new ClaimsPrincipal(identity);
    }

    /// <summary>Đăng nhập: phát cookie cho Customer.</summary>
    public static Task SignInAsync(HttpContext http, Customer customer, bool isPersistent)
        => http.SignInAsync(Scheme, BuildPrincipal(customer),
            new AuthenticationProperties { IsPersistent = isPersistent });

    /// <summary>Đăng xuất: xoá cookie storefront.</summary>
    public static Task SignOutAsync(HttpContext http) => http.SignOutAsync(Scheme);
}
