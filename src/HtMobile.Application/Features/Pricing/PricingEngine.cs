using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Pricing.Dtos;
using HtMobile.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Pricing;

/// <summary>
/// Pricing engine (SPEC §2): lấy giá theo vùng từ DB + áp khuyến mãi (qua <see cref="PriceCalculator"/>),
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

    public Task<EffectivePrice> GetEffectivePriceAsync(long variantId, long regionId, CancellationToken ct = default)
    {
        return _cache.GetOrCreateAsync(
            CacheKeys.VariantPrice(variantId, regionId),
            () => ComputeAsync(variantId, regionId, ct),
            CacheTtl,
            ct);
    }

    public Task InvalidateAsync(long variantId, long regionId, CancellationToken ct = default)
        => _cache.RemoveAsync(CacheKeys.VariantPrice(variantId, regionId), ct);

    private async Task<EffectivePrice> ComputeAsync(long variantId, long regionId, CancellationToken ct)
    {
        var priceRow = await _db.PricesByRegion
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.VariantId == variantId && p.RegionId == regionId, ct);

        decimal listPrice;
        decimal? compareAt;

        if (priceRow is not null)
        {
            listPrice = priceRow.Price;
            compareAt = priceRow.CompareAtPrice;
        }
        else
        {
            // Fallback: chưa cấu hình giá theo vùng → dùng BasePrice của variant.
            var variant = await _db.ProductVariants
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == variantId, ct);
            listPrice = variant?.BasePrice ?? 0m;
            compareAt = null;
        }

        var now = _clock.Now;
        var promotions = await _db.Promotions
            .AsNoTracking()
            .Where(p => p.StartsAt <= now && p.EndsAt >= now)
            .ToListAsync(ct);

        return PriceCalculator.Calculate(variantId, regionId, listPrice, compareAt, promotions, now);
    }
}
