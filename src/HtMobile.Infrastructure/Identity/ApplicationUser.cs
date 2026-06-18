using Microsoft.AspNetCore.Identity;

namespace HtMobile.Infrastructure.Identity;

/// <summary>
/// Tài khoản đăng nhập <b>Admin</b> (ASP.NET Identity, API JWT) — ADR 0005. Khách hàng storefront ở bảng
/// <c>Customers</c> riêng, KHÔNG còn là <c>ApplicationUser</c>. Khóa chính <see cref="long"/>.
/// </summary>
public class ApplicationUser : IdentityUser<long>
{
    public string? FullName { get; set; }
}
