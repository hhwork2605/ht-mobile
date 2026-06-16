namespace HtMobile.Application.Features.Cart.Dtos;

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
    public decimal Total { get; init; }
    public bool IsEmpty => Items.Count == 0;
}
