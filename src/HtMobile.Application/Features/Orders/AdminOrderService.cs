using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Orders.Dtos;
using HtMobile.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Orders;

/// <summary>Quản lý đơn cho Admin (P5-01): danh sách + lọc trạng thái, chi tiết, đổi trạng thái (validate luồng).</summary>
public class AdminOrderService
{
    private readonly IApplicationDbContext _db;

    public AdminOrderService(IApplicationDbContext db) => _db = db;

    public async Task<AdminOrderListDto> GetListAsync(OrderStatus? filter, CancellationToken ct = default)
    {
        var counts = await _db.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var query = _db.Orders.AsNoTracking();
        if (filter is not null) query = query.Where(o => o.Status == filter);

        var rows = await query
            .OrderByDescending(o => o.Id)
            .Select(o => new
            {
                o.Id,
                o.Total,
                o.PaymentMethod,
                o.Status,
                o.CreatedAt,
                Recipient = o.Shipment != null ? o.Shipment.Address : null,
                Items = o.Items.Select(i => new { i.Quantity, Name = i.Variant.Product.Name }).ToList()
            })
            .ToListAsync(ct);

        var orders = rows.Select(o => new AdminOrderRow
        {
            Id = o.Id,
            Total = o.Total,
            PaymentMethod = o.PaymentMethod,
            Status = o.Status,
            CreatedAt = o.CreatedAt,
            Recipient = o.Recipient ?? "—",
            ItemsSummary = SummarizeItems(o.Items.Select(i => $"{i.Name} ×{i.Quantity}").ToList())
        }).ToList();

        return new AdminOrderListDto
        {
            Orders = orders,
            Counts = counts.ToDictionary(c => c.Status, c => c.Count),
            TotalCount = counts.Sum(c => c.Count),
            Filter = filter
        };
    }

    // Tóm tắt tối đa 2 dòng đầu + "… +N" để không vỡ ô bảng.
    private static string SummarizeItems(IReadOnlyList<string> lines)
        => lines.Count <= 2
            ? string.Join(", ", lines)
            : $"{string.Join(", ", lines.Take(2))} … +{lines.Count - 2} SP";

    public async Task<AdminOrderDetailDto?> GetDetailAsync(long id, CancellationToken ct = default)
    {
        var o = await _db.Orders
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Status,
                x.CreatedAt,
                x.Total,
                x.PaymentMethod,
                x.CustomerId,
                Address = x.Shipment != null ? x.Shipment.Address : null,
                Items = x.Items.Select(i => new
                {
                    i.Quantity,
                    i.UnitPrice,
                    Name = i.Variant.Product.Name,
                    i.Variant.Storage,
                    i.Variant.Color
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);
        if (o is null) return null;

        return new AdminOrderDetailDto
        {
            Id = o.Id,
            Status = o.Status,
            CreatedAt = o.CreatedAt,
            Total = o.Total,
            PaymentMethod = o.PaymentMethod,
            CustomerId = o.CustomerId,
            ShipmentAddress = o.Address,
            Items = o.Items.Select(i => new AdminOrderLine
            {
                ProductName = i.Name,
                VariantText = string.Join(" · ", new[] { i.Color, i.Storage }.Where(s => !string.IsNullOrWhiteSpace(s))),
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.UnitPrice * i.Quantity
            }).ToList(),
            AllowedNext = OrderStatusTransition.AllowedNext(o.Status)
        };
    }

    public async Task<ChangeStatusResult> ChangeStatusAsync(long id, OrderStatus newStatus, CancellationToken ct = default)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null) return ChangeStatusResult.NotFound;
        if (!OrderStatusTransition.CanTransition(order.Status, newStatus)) return ChangeStatusResult.InvalidTransition;

        order.Status = newStatus;
        await _db.SaveChangesAsync(ct);
        return ChangeStatusResult.Ok;
    }
}
