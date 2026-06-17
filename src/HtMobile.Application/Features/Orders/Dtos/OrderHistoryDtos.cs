using HtMobile.Domain.Enums;

namespace HtMobile.Application.Features.Orders.Dtos;

/// <summary>Một dòng trong đơn (giá lấy từ snapshot OrderItem.UnitPrice — đơn cũ giữ nguyên giá đã trả).</summary>
public record OrderLineView
{
    public string ProductName { get; init; } = string.Empty;
    public string VariantText { get; init; } = string.Empty;
    public string? VariantSlug { get; init; }
    public string? ThumbnailUrl { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}

/// <summary>Tóm tắt 1 đơn cho lịch sử tài khoản.</summary>
public record OrderSummaryDto
{
    public long Id { get; init; }
    public string Code => $"SD{Id:D6}";
    public DateTime CreatedAt { get; init; }
    public OrderStatus Status { get; init; }
    public decimal Total { get; init; }
    public IReadOnlyList<OrderLineView> Items { get; init; } = Array.Empty<OrderLineView>();
}
