using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Orders.Dtos;
using HtMobile.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Orders;

/// <summary>Lịch sử đơn hàng của 1 khách. Giá hiển thị = snapshot lúc đặt (OrderItem.UnitPrice), KHÔNG định giá lại.</summary>
public class OrderHistoryService
{
    private readonly IApplicationDbContext _db;

    public OrderHistoryService(IApplicationDbContext db) => _db = db;

    /// <summary>Đơn của 1 khách, mới nhất trước.</summary>
    public Task<IReadOnlyList<OrderSummaryDto>> GetMyOrdersAsync(long customerId, CancellationToken ct = default)
        => QuerySummariesAsync(_db.Orders.Where(o => o.CustomerId == customerId).OrderByDescending(o => o.Id), ct);

    /// <summary>
    /// Tra cứu đơn cho khách vãng lai: mã đơn (SDxxxxxx) + SĐT. Trả đơn nếu mã hợp lệ &amp; SĐT khớp
    /// thông tin nhận hàng (Shipment.Address chứa SĐT). null nếu không khớp — không tiết lộ đơn tồn tại.
    /// </summary>
    public async Task<OrderSummaryDto?> LookupAsync(string code, string phone, CancellationToken ct = default)
    {
        var id = ParseCode(code);
        var digits = Digits(phone);
        if (id is null || digits.Length < 6) return null;

        var ship = await _db.Orders.AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => o.Shipment != null ? o.Shipment.Address : null)
            .FirstOrDefaultAsync(ct);
        if (ship is null || !Digits(ship).Contains(digits)) return null;

        var list = await QuerySummariesAsync(_db.Orders.Where(o => o.Id == id), ct);
        return list.Count > 0 ? list[0] : null;
    }

    private async Task<IReadOnlyList<OrderSummaryDto>> QuerySummariesAsync(IQueryable<Order> query, CancellationToken ct)
    {
        var orders = await query
            .AsNoTracking()
            .Select(o => new
            {
                o.Id,
                o.CreatedAt,
                o.Status,
                o.Total,
                Items = o.Items.Select(i => new
                {
                    i.Quantity,
                    i.UnitPrice,
                    // ProductName = model cha (nếu biến thể có cha), ảnh gallery cũng ở model cha.
                    ProductName = i.Product.Parent != null ? i.Product.Parent.Name : i.Product.Name,
                    Attrs = i.Product.Attributes.OrderBy(a => a.Attribute.SortOrder).ThenBy(a => a.AttributeId).Select(a => a.Value).ToList(),
                    VariantSlug = i.Product.Slug,
                    Thumbnail = i.Product.Parent!.Images
                        .OrderBy(im => im.SortOrder)
                        .Select(im => im.Url)
                        .FirstOrDefault()
                }).ToList()
            })
            .ToListAsync(ct);

        return orders.Select(o => new OrderSummaryDto
        {
            Id = o.Id,
            CreatedAt = o.CreatedAt,
            Status = o.Status,
            Total = o.Total,
            Items = o.Items.Select(i => new OrderLineView
            {
                ProductName = i.ProductName,
                VariantText = string.Join(" · ", i.Attrs),
                VariantSlug = i.VariantSlug,
                ThumbnailUrl = i.Thumbnail,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.UnitPrice * i.Quantity
            }).ToList()
        }).ToList();
    }

    /// <summary>Tách số id từ mã đơn (chấp nhận "SD000005", "000005", "5").</summary>
    private static long? ParseCode(string? code)
    {
        var s = Digits(code ?? string.Empty);
        return long.TryParse(s, out var id) && id > 0 ? id : null;
    }

    private static string Digits(string s) => new(s.Where(char.IsDigit).ToArray());
}
