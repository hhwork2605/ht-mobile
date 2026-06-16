using Microsoft.AspNetCore.Identity;

namespace HtMobile.Infrastructure.Identity;

/// <summary>
/// Người dùng/khách hàng. Gộp vai trò "Customer" của SPEC §7 (email/phone) vào Identity.
/// Khóa chính <see cref="long"/> để FK đồng nhất với các entity domain.
/// </summary>
public class ApplicationUser : IdentityUser<long>
{
    public string? FullName { get; set; }
}
