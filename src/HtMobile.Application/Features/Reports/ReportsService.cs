using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Reports.Dtos;
using HtMobile.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Reports;

/// <summary>Báo cáo bán hàng: KPI + doanh thu theo ngày + sản phẩm bán chạy. Doanh thu = đơn không huỷ/hoàn.</summary>
public class ReportsService
{
    private static readonly OrderStatus[] RevenueStatuses =
        { OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered };

    private readonly IApplicationDbContext _db;

    public ReportsService(IApplicationDbContext db) => _db = db;

    public async Task<SalesReportDto> GetSalesAsync(DateTime from, DateTime to, int topN = 10, CancellationToken ct = default)
    {
        var fromDate = from.Date;
        var toExclusive = to.Date.AddDays(1);   // bao trọn ngày 'to'

        var orders = await _db.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt < toExclusive)
            .Select(o => new { o.Total, o.Status, o.CreatedAt })
            .ToListAsync(ct);

        var revenueOrders = orders.Where(o => RevenueStatuses.Contains(o.Status)).ToList();
        var revenueTotal = revenueOrders.Sum(o => o.Total);
        var cancelled = orders.Count(o => o.Status is OrderStatus.Cancelled or OrderStatus.Refunded);

        // Doanh thu theo ngày — điền đủ mọi ngày trong khoảng (kể cả ngày 0đ) cho biểu đồ liền mạch.
        var byDayMap = revenueOrders
            .GroupBy(o => o.CreatedAt.Date)
            .ToDictionary(g => g.Key, g => (Revenue: g.Sum(x => x.Total), Orders: g.Count()));
        var byDay = new List<DayPoint>();
        for (var d = fromDate; d < toExclusive; d = d.AddDays(1))
        {
            var hit = byDayMap.TryGetValue(d, out var v) ? v : (Revenue: 0m, Orders: 0);
            byDay.Add(new DayPoint(d, hit.Revenue, hit.Orders));
        }

        // Sản phẩm bán chạy — từ dòng đơn của các đơn tính doanh thu trong khoảng.
        var itemRows = await _db.OrderItems
            .AsNoTracking()
            .Where(i => i.Order.CreatedAt >= fromDate && i.Order.CreatedAt < toExclusive
                && RevenueStatuses.Contains(i.Order.Status))
            .Select(i => new
            {
                i.ProductId,
                i.Quantity,
                i.UnitPrice,
                Name = i.Product.Parent != null ? i.Product.Parent.Name : i.Product.Name,
                Attrs = i.Product.Attributes.OrderBy(a => a.Attribute.SortOrder).ThenBy(a => a.AttributeId).Select(a => a.Value).ToList(),
            })
            .ToListAsync(ct);

        var topProducts = itemRows
            .GroupBy(i => new { i.ProductId, i.Name, Label = string.Join(" · ", i.Attrs) })
            .Select(g => new TopProductRow
            {
                ProductId = g.Key.ProductId,
                Name = g.Key.Name,
                VariantLabel = g.Key.Label,
                QuantitySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.UnitPrice * x.Quantity),
            })
            .OrderByDescending(p => p.QuantitySold).ThenByDescending(p => p.Revenue)
            .Take(topN)
            .ToList();

        return new SalesReportDto
        {
            From = fromDate,
            To = to.Date,
            RevenueTotal = revenueTotal,
            OrdersCount = orders.Count,
            AvgOrderValue = revenueOrders.Count > 0 ? Math.Round(revenueTotal / revenueOrders.Count, 0) : 0m,
            CancelRate = orders.Count > 0 ? Math.Round(cancelled * 100.0 / orders.Count, 1) : 0,
            ByDay = byDay,
            TopProducts = topProducts,
        };
    }
}
