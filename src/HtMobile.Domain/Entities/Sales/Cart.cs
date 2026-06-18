using HtMobile.Domain.Common;

namespace HtMobile.Domain.Entities.Sales;

/// <summary>Giỏ hàng. Khách vãng lai dùng SessionId; khách đăng nhập dùng CustomerId. SPEC §7.</summary>
public class Cart : BaseAuditableEntity
{
    public long? CustomerId { get; set; }
    public string? SessionId { get; set; }

    /// <summary>Mã giảm giá đang áp cho giỏ (null = không có). Revalidate khi đọc giỏ/checkout.</summary>
    public string? CouponCode { get; set; }

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
