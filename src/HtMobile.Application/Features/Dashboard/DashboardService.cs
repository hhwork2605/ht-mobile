using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Dashboard.Dtos;
using HtMobile.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Dashboard;

/// <summary>Số liệu tổng quan cho trang Bảng điều khiển admin (KPI + đơn gần đây + phân bổ trạng thái).</summary>
public class DashboardService
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTime _clock;

    public DashboardService(IApplicationDbContext db, IDateTime clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<DashboardStatsDto> GetAsync(CancellationToken ct = default)
    {
        var today = _clock.Now.Date;
        // Doanh thu = đơn không huỷ/hoàn tiền.
        var revenueStatuses = new[] { OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered };

        var orders = await _db.Orders
            .AsNoTracking()
            .Select(o => new { o.Id, o.Total, o.Status, o.CreatedAt, o.CustomerId })
            .ToListAsync(ct);

        var revenueOrders = orders.Where(o => revenueStatuses.Contains(o.Status)).ToList();

        var statusCounts = orders
            .GroupBy(o => o.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        var recent = await _db.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.Id)
            .Take(6)
            .Select(o => new RecentOrderDto(
                o.Id,
                o.Shipment != null ? o.Shipment.Address : "—",
                o.Total,
                o.Status,
                o.CreatedAt))
            .ToListAsync(ct);

        var productsCount = await _db.Products.CountAsync(p => p.ProductParentId == null, ct);

        // Doanh thu 14 ngày (điền đủ ngày cho mini chart).
        var chartFrom = today.AddDays(-13);
        var revByDate = revenueOrders.Where(o => o.CreatedAt.Date >= chartFrom)
            .GroupBy(o => o.CreatedAt.Date).ToDictionary(g => g.Key, g => g.Sum(x => x.Total));
        var revenueByDay = new List<DashboardDayPoint>();
        for (var d = chartFrom; d <= today; d = d.AddDays(1))
            revenueByDay.Add(new DashboardDayPoint(d, revByDate.GetValueOrDefault(d)));

        // Top 5 sản phẩm bán chạy 30 ngày.
        var since = today.AddDays(-29);
        var itemRows = await _db.OrderItems
            .AsNoTracking()
            .Where(i => i.Order.CreatedAt >= since && revenueStatuses.Contains(i.Order.Status))
            .Select(i => new
            {
                i.ProductId, i.Quantity, i.UnitPrice,
                Name = i.Product.Parent != null ? i.Product.Parent.Name : i.Product.Name,
                Attrs = i.Product.Attributes.OrderBy(a => a.Attribute.SortOrder).ThenBy(a => a.AttributeId).Select(a => a.Value).ToList(),
            })
            .ToListAsync(ct);
        var topProducts = itemRows
            .GroupBy(i => new { i.ProductId, i.Name, Label = string.Join(" · ", i.Attrs) })
            .Select(g => new DashboardTopProduct
            {
                ProductId = g.Key.ProductId, Name = g.Key.Name, VariantLabel = g.Key.Label,
                QuantitySold = g.Sum(x => x.Quantity), Revenue = g.Sum(x => x.UnitPrice * x.Quantity),
            })
            .OrderByDescending(p => p.QuantitySold).ThenByDescending(p => p.Revenue)
            .Take(5).ToList();

        // Tồn kho thấp: biến thể (Product con) đang bán có tổng tồn ≤ ngưỡng (gồm = 0).
        const int threshold = 5;
        var stockByProduct = await _db.Inventories
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Total = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.Total, ct);
        var sellable = await _db.Products
            .AsNoTracking()
            .Where(p => p.ProductParentId != null && p.Status == ProductStatus.Active)
            .Select(p => new
            {
                p.Id,
                Name = p.Parent!.Name,
                Attrs = p.Attributes.OrderBy(a => a.Attribute.SortOrder).ThenBy(a => a.AttributeId).Select(a => a.Value).ToList(),
            })
            .ToListAsync(ct);
        var lowAll = sellable
            .Select(v => new LowStockRow { ProductId = v.Id, Name = v.Name, VariantLabel = string.Join(" · ", v.Attrs), TotalQuantity = stockByProduct.GetValueOrDefault(v.Id) })
            .Where(v => v.TotalQuantity <= threshold)
            .OrderBy(v => v.TotalQuantity).ThenBy(v => v.Name)
            .ToList();

        return new DashboardStatsDto
        {
            RevenueTotal = revenueOrders.Sum(o => o.Total),
            RevenueToday = revenueOrders.Where(o => o.CreatedAt.Date == today).Sum(o => o.Total),
            OrdersTotal = orders.Count,
            OrdersToday = orders.Count(o => o.CreatedAt.Date == today),
            ProductsCount = productsCount,
            CustomersCount = orders.Where(o => o.CustomerId != null).Select(o => o.CustomerId).Distinct().Count(),
            StatusCounts = statusCounts,
            RecentOrders = recent,
            RevenueByDay = revenueByDay,
            TopProducts = topProducts,
            LowStock = lowAll.Take(8).ToList(),
            LowStockCount = lowAll.Count,
            LowStockThreshold = threshold,
        };
    }
}
