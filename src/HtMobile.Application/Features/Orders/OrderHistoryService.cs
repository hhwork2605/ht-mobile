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
                    i.VariantId,
                    i.Quantity,
                    i.UnitPrice,
                    ProductName = i.Variant.Product.Name,
                    i.Variant.Storage,
                    i.Variant.Color,
                    VariantSlug = i.Variant.Slug,
                    // Ưu tiên ảnh đúng variant; fallback ảnh chung của sản phẩm (bỏ ảnh của variant khác).
                    Thumbnail = i.Variant.Product.Images
                        .Where(im => im.VariantId == null || im.VariantId == i.VariantId)
                        .OrderBy(im => im.VariantId == i.VariantId ? 0 : 1)
                        .ThenBy(im => im.SortOrder)
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
                VariantText = string.Join(" · ",
                    new[] { i.Color, i.Storage }.Where(s => !string.IsNullOrWhiteSpace(s))),
                VariantSlug = i.VariantSlug,
                ThumbnailUrl = i.Thumbnail,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.UnitPrice * i.Quantity
            }).ToList()
        }).ToList();
    }
}
