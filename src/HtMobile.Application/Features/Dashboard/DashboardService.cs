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
        };
    }
}
