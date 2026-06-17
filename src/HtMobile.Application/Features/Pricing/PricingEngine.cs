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

    public Task<EffectivePrice> GetEffectivePriceAsync(long variantId, CancellationToken ct = default)
    {
        return _cache.GetOrCreateAsync(
            CacheKeys.VariantPrice(variantId),
            () => ComputeAsync(variantId, ct),
            CacheTtl,
            ct);
    }

    public Task InvalidateAsync(long variantId, CancellationToken ct = default)
        => _cache.RemoveAsync(CacheKeys.VariantPrice(variantId), ct);

    private async Task<EffectivePrice> ComputeAsync(long variantId, CancellationToken ct)
    {
        // Giá lấy trực tiếp từ variant (giá duy nhất, không phân theo vùng) + ngữ cảnh để lọc KM theo điều kiện.
        var variant = await _db.ProductVariants
            .AsNoTracking()
            .Where(v => v.Id == variantId)
            .Select(v => new { v.BasePrice, v.CompareAtPrice, v.ProductId, v.Product.CategoryId })
            .FirstOrDefaultAsync(ct);

        var now = _clock.Now;
        if (variant is null)
            return PriceCalculator.Calculate(variantId, 0m, null, Array.Empty<Domain.Entities.Pricing.Promotion>(), now);

        var ctx = new PricingContext(variantId, variant.ProductId, variant.CategoryId);

        var active = await _db.Promotions
            .AsNoTracking()
            .Where(p => p.StartsAt <= now && p.EndsAt >= now)
            .ToListAsync(ct);

        // Lọc theo ConditionsJson (in-memory; danh sách KM đang chạy nhỏ).
        var applicable = active.Where(p => PromotionConditions.Matches(p.ConditionsJson, ctx)).ToList();

        return PriceCalculator.Calculate(variantId, variant.BasePrice, variant.CompareAtPrice, applicable, now);
    }
}
