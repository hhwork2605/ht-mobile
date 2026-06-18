using HtMobile.Domain.Common;
using HtMobile.Domain.Entities.Sales;

namespace HtMobile.Domain.Entities.Customers;

/// <summary>
/// Khách hàng storefront (SPEC §7) — tài khoản mua hàng phía cửa hàng, TÁCH KHỎI ASP.NET Identity.
/// Identity (<c>ApplicationUser</c>) nay chỉ dùng cho đăng nhập Admin (API JWT). Xem ADR 0004.
/// Đăng nhập bằng email + mật khẩu (băm qua <c>IPasswordHasher</c>); đơn hàng gắn qua <see cref="Order.CustomerId"/>.
/// </summary>
public class Customer : BaseAuditableEntity
{
    /// <summary>Email đăng nhập (chuẩn hoá thường, UNIQUE).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Mật khẩu đã băm (PHC string từ <c>IPasswordHasher</c>).</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string? FullName { get; set; }
    public string? Phone { get; set; }

    /// <summary>Băm (SHA-256 hex) của token đặt lại mật khẩu; null = không có yêu cầu reset đang chờ.</summary>
    public string? ResetTokenHash { get; set; }

    /// <summary>Thời điểm token reset hết hạn (UTC); null nếu không có.</summary>
    public DateTime? ResetTokenExpiresAt { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
