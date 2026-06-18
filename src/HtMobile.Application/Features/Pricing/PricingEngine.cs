using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Pricing.Dtos;
using HtMobile.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Pricing;

/// <summary>
/// Pricing engine (SPEC §2): lấy giá từ variant + áp khuyến mãi (qua <see cref="PriceCalculator"/>),
/// cache kết quả ở Redis. Mọi nơi cần giá phải đi qua interface này.
/// </summary>
public class PricingEngine : IPricingService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    private readonly IApplicationDbContext _db;
    private readonly ICacheService _cache;
    private readonly IDateTime _clock;

    public PricingEngine(IApplicationDbContext db, ICacheService cache, IDateTime clock)
    {
        _db = db;
        _cache = cache;
        _clock = clock;
    }

    public Task<EffectivePrice> GetEffectivePriceAsync(long productId, CancellationToken ct = default)
    {
        return _cache.GetOrCreateAsync(
            CacheKeys.ProductPrice(productId),
            () => ComputeAsync(productId, ct),
            CacheTtl,
            ct);
    }

    public Task InvalidateAsync(long productId, CancellationToken ct = default)
        => _cache.RemoveAsync(CacheKeys.ProductPrice(productId), ct);

    private async Task<EffectivePrice> ComputeAsync(long productId, CancellationToken ct)
    {
        // Giá lấy trực tiếp từ Product (biến thể con) + ngữ cảnh (parent + category) để lọc KM theo điều kiện.
        var product = await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => new { p.BasePrice, p.CompareAtPrice, p.ProductParentId, p.CategoryId })
            .FirstOrDefaultAsync(ct);

        var now = _clock.Now;
        if (product is null)
            return PriceCalculator.Calculate(productId, 0m, null, Array.Empty<Domain.Entities.Pricing.Promotion>(), now);

        var ctx = new PricingContext(productId, product.ProductParentId, product.CategoryId);

        // KM có Code = voucher (chỉ áp khi khách nhập mã ở giỏ) → KHÔNG auto-apply vào giá sản phẩm.
        var active = await _db.Promotions
            .AsNoTracking()
            .Where(p => p.Code == null && p.StartsAt <= now && p.EndsAt >= now)
            .ToListAsync(ct);

        // Lọc theo ConditionsJson (in-memory; danh sách KM đang chạy nhỏ).
        var applicable = active.Where(p => PromotionConditions.Matches(p.ConditionsJson, ctx)).ToList();

        return PriceCalculator.Calculate(productId, product.BasePrice, product.CompareAtPrice, applicable, now);
    }
}
