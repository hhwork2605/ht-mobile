using HtMobile.Domain.Enums;

namespace HtMobile.Application.Features.Dashboard.Dtos;

/// <summary>Một đơn gần đây hiển thị ở dashboard. Recipient = địa chỉ/thông tin nhận hàng.</summary>
public record RecentOrderDto(long Id, string Recipient, decimal Total, OrderStatus Status, DateTime CreatedAt)
{
    public string Code => $"SD{Id:D6}";
}

/// <summary>1 điểm doanh thu theo ngày (mini chart 14 ngày).</summary>
public record DashboardDayPoint(DateTime Date, decimal Revenue);

/// <summary>Sản phẩm bán chạy (30 ngày) trên dashboard.</summary>
public record DashboardTopProduct
{
    public long ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string VariantLabel { get; init; } = string.Empty;
    public int QuantitySold { get; init; }
    public decimal Revenue { get; init; }
}

/// <summary>Biến thể (Product con) tồn kho thấp/hết.</summary>
public record LowStockRow
{
    public long ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string VariantLabel { get; init; } = string.Empty;
    public int TotalQuantity { get; init; }
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

    /// <summary>Doanh thu 14 ngày gần nhất (điền đủ ngày).</summary>
    public IReadOnlyList<DashboardDayPoint> RevenueByDay { get; init; } = Array.Empty<DashboardDayPoint>();
    /// <summary>Top 5 sản phẩm bán chạy 30 ngày.</summary>
    public IReadOnlyList<DashboardTopProduct> TopProducts { get; init; } = Array.Empty<DashboardTopProduct>();
    /// <summary>Biến thể tồn kho thấp (≤ ngưỡng), tối đa vài dòng.</summary>
    public IReadOnlyList<LowStockRow> LowStock { get; init; } = Array.Empty<LowStockRow>();
    public int LowStockCount { get; init; }
    public int LowStockThreshold { get; init; }
}
