using HtMobile.Application.Common.Interfaces;
using HtMobile.Domain.Entities.Reviews;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Catalog;

/// <summary>Kết quả gửi đánh giá.</summary>
public enum ReviewResult { Ok, ProductNotFound, InvalidRating }

/// <summary>
/// Đánh giá sản phẩm của khách hàng (storefront). Gắn review về <b>model</b> (sản phẩm cha) để gộp đúng
/// với phần tổng hợp ở PDP. Mỗi khách 1 đánh giá / 1 model (gửi lại = cập nhật).
/// </summary>
public class ReviewService
{
    private readonly IApplicationDbContext _db;

    public ReviewService(IApplicationDbContext db) => _db = db;

    public async Task<ReviewResult> AddOrUpdateAsync(long productId, long customerId, int rating, string? content, CancellationToken ct = default)
    {
        if (rating is < 1 or > 5) return ReviewResult.InvalidRating;

        var product = await _db.Products.AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => new { p.Id, p.ProductParentId })
            .FirstOrDefaultAsync(ct);
        if (product is null) return ReviewResult.ProductNotFound;

        var modelId = product.ProductParentId ?? product.Id;   // luôn gắn về model
        var trimmed = string.IsNullOrWhiteSpace(content) ? null : content.Trim();

        var existing = await _db.Reviews.FirstOrDefaultAsync(r => r.ProductId == modelId && r.CustomerId == customerId, ct);
        if (existing is null)
            _db.Reviews.Add(new Review { ProductId = modelId, CustomerId = customerId, Rating = rating, Content = trimmed });
        else
        {
            existing.Rating = rating;
            existing.Content = trimmed;
        }

        await _db.SaveChangesAsync(ct);
        return ReviewResult.Ok;
    }
}
