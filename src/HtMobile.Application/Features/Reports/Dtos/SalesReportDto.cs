namespace HtMobile.Application.Features.Reports.Dtos;

/// <summary>1 điểm doanh thu theo ngày (cho biểu đồ).</summary>
public record DayPoint(DateTime Date, decimal Revenue, int Orders);

/// <summary>1 dòng sản phẩm bán chạy.</summary>
public record TopProductRow
{
    public long ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string VariantLabel { get; init; } = string.Empty;
    public int QuantitySold { get; init; }
    public decimal Revenue { get; init; }
}

/// <summary>Báo cáo bán hàng theo khoảng thời gian.</summary>
public record SalesReportDto
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
    public decimal RevenueTotal { get; init; }
    public int OrdersCount { get; init; }
    public decimal AvgOrderValue { get; init; }
    /// <summary>Tỷ lệ đơn huỷ/hoàn trên tổng đơn (%).</summary>
    public double CancelRate { get; init; }
    public IReadOnlyList<DayPoint> ByDay { get; init; } = Array.Empty<DayPoint>();
    public IReadOnlyList<TopProductRow> TopProducts { get; init; } = Array.Empty<TopProductRow>();
}
