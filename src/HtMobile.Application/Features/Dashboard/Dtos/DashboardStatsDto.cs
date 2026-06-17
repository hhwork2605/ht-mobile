using HtMobile.Domain.Enums;

namespace HtMobile.Application.Features.Dashboard.Dtos;

/// <summary>Một đơn gần đây hiển thị ở dashboard. Recipient = địa chỉ/thông tin nhận hàng.</summary>
public record RecentOrderDto(long Id, string Recipient, decimal Total, OrderStatus Status, DateTime CreatedAt)
{
    public string Code => $"SD{Id:D6}";
}

/// <summary>KPI + dữ liệu tổng quan cho Bảng điều khiển admin.</summary>
public record DashboardStatsDto
{
    public decimal RevenueTotal { get; init; }
    public decimal RevenueToday { get; init; }
    public int OrdersTotal { get; init; }
    public int OrdersToday { get; init; }
    public int ProductsCount { get; init; }
    public int CustomersCount { get; init; }
    public IReadOnlyDictionary<OrderStatus, int> StatusCounts { get; init; } = new Dictionary<OrderStatus, int>();
    public IReadOnlyList<RecentOrderDto> RecentOrders { get; init; } = Array.Empty<RecentOrderDto>();
}
