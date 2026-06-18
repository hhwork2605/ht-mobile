using HtMobile.Application.Features.Pricing;

namespace HtMobile.Application.Features.Cart.Dtos;

/// <summary>Kết quả áp mã giảm giá + giỏ sau khi áp (để render lại).</summary>
public record ApplyCouponResult(CouponOutcome Outcome, CartDto Cart)
{
    public bool Success => Outcome == CouponOutcome.Ok;
}

/// <summary>Một dòng trong giỏ (đã có giá hiệu lực + thông tin hiển thị).</summary>
public record CartLineDto
{
    public long Id { get; init; }
    public long VariantId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    /// <summary>"Màu · Dung lượng" (vd "Trắng · 256GB").</summary>
    public string VariantText { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public string VariantSlug { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal LineTotal { get; init; }
}

/// <summary>Toàn bộ giỏ + tóm tắt tiền (đơn giá lấy qua IPricingService).</summary>
public record CartDto
{
    public IReadOnlyList<CartLineDto> Items { get; init; } = Array.Empty<CartLineDto>();
    public int Count { get; init; }
    public decimal Subtotal { get; init; }
    public decimal ShippingFee { get; init; }

    /// <summary>Mã giảm giá đang áp (null = không có / không còn hợp lệ).</summary>
    public string? CouponCode { get; init; }

    /// <summary>Số tiền giảm từ mã (0 nếu không áp được).</summary>
    public decimal Discount { get; init; }

    /// <summary>Tổng phải trả = Subtotal + ShippingFee − Discount.</summary>
    public decimal Total { get; init; }
    public bool IsEmpty => Items.Count == 0;
}
