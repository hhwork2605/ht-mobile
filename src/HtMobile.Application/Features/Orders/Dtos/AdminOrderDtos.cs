using HtMobile.Domain.Enums;

namespace HtMobile.Application.Features.Orders.Dtos;

/// <summary>Một dòng trong bảng đơn (admin).</summary>
public record AdminOrderRow
{
    public long Id { get; init; }
    public string Code => $"SD{Id:D6}";
    public string Recipient { get; init; } = string.Empty;
    public string ItemsSummary { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public string? PaymentMethod { get; init; }
    public OrderStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>Danh sách đơn + đếm theo trạng thái (cho tab lọc).</summary>
public record AdminOrderListDto
{
    public IReadOnlyList<AdminOrderRow> Orders { get; init; } = Array.Empty<AdminOrderRow>();
    public IReadOnlyDictionary<OrderStatus, int> Counts { get; init; } = new Dictionary<OrderStatus, int>();
    public int TotalCount { get; init; }
    public OrderStatus? Filter { get; init; }
}

public record AdminOrderLine
{
    public string ProductName { get; init; } = string.Empty;
    public string VariantText { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}

/// <summary>Chi tiết đơn (admin) + các trạng thái có thể chuyển tới.</summary>
public record AdminOrderDetailDto
{
    public long Id { get; init; }
    public string Code => $"SD{Id:D6}";
    public OrderStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public decimal Total { get; init; }
    public string? PaymentMethod { get; init; }
    public string? ShipmentAddress { get; init; }
    public long? CustomerId { get; init; }
    public IReadOnlyList<AdminOrderLine> Items { get; init; } = Array.Empty<AdminOrderLine>();
    public IReadOnlyList<OrderStatus> AllowedNext { get; init; } = Array.Empty<OrderStatus>();
}

/// <summary>Kết quả đổi trạng thái đơn.</summary>
public enum ChangeStatusResult { Ok, NotFound, InvalidTransition }
