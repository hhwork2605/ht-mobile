using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Orders.Dtos;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Orders;

/// <summary>Lịch sử đơn hàng của 1 khách. Giá hiển thị = snapshot lúc đặt (OrderItem.UnitPrice), KHÔNG định giá lại.</summary>
public class OrderHistoryService
{
    private readonly IApplicationDbContext _db;

    public OrderHistoryService(IApplicationDbContext db) => _db = db;

    /// <summary>Đơn của 1 khách, mới nhất trước.</summary>
    public async Task<IReadOnlyList<OrderSummaryDto>> GetMyOrdersAsync(long customerId, CancellationToken ct = default)
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.Id)
            .Select(o => new
            {
                o.Id,
                o.CreatedAt,
                o.Status,
                o.Total,
                Items = o.Items.Select(i => new
                {
                    i.ProductId,
                    i.Quantity,
                    i.UnitPrice,
                    // ProductName = model cha (nếu biến thể có cha), ảnh gallery cũng ở model cha.
                    ProductName = i.Product.Parent != null ? i.Product.Parent.Name : i.Product.Name,
                    Attrs = i.Product.Attributes.OrderBy(a => a.Attribute.SortOrder).ThenBy(a => a.AttributeId).Select(a => a.Value).ToList(),
                    VariantSlug = i.Product.Slug,
                    // Ảnh gallery ở model cha (biến thể con không có ảnh riêng).
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
}
