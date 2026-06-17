using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Pricing.Dtos;
using HtMobile.Domain.Entities.Pricing;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Pricing;

/// <summary>CRUD khuyến mãi cho admin. Pricing engine đọc các KM đang hiệu lực + lọc theo ConditionsJson.</summary>
public class AdminPromotionService
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTime _clock;

    public AdminPromotionService(IApplicationDbContext db, IDateTime clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<IReadOnlyList<AdminPromotionRow>> GetListAsync(CancellationToken ct = default)
    {
        var now = _clock.Now;
        return await _db.Promotions
            .AsNoTracking()
            .OrderByDescending(p => p.Id)
            .Select(p => new AdminPromotionRow
            {
                Id = p.Id,
                Name = p.Name,
                Type = p.Type,
                Value = p.Value,
                StartsAt = p.StartsAt,
                EndsAt = p.EndsAt,
                IsActiveNow = p.StartsAt <= now && p.EndsAt >= now,
            })
            .ToListAsync(ct);
    }

    public async Task<AdminPromotionDetail?> GetAsync(long id, CancellationToken ct = default)
        => await _db.Promotions
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new AdminPromotionDetail
            {
                Id = p.Id, Name = p.Name, Type = p.Type, Value = p.Value,
                StartsAt = p.StartsAt, EndsAt = p.EndsAt, ConditionsJson = p.ConditionsJson,
            })
            .FirstOrDefaultAsync(ct);

    public async Task<long> CreateAsync(PromotionInput input, CancellationToken ct = default)
    {
        var promo = new Promotion
        {
            Name = input.Name.Trim(),
            Type = input.Type,
            Value = input.Value,
            StartsAt = input.StartsAt,
            EndsAt = input.EndsAt,
            ConditionsJson = string.IsNullOrWhiteSpace(input.ConditionsJson) ? null : input.ConditionsJson,
        };
        _db.Promotions.Add(promo);
        await _db.SaveChangesAsync(ct);
        return promo.Id;
    }

    public async Task<bool> UpdateAsync(long id, PromotionInput input, CancellationToken ct = default)
    {
        var promo = await _db.Promotions.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (promo is null) return false;

        promo.Name = input.Name.Trim();
        promo.Type = input.Type;
        promo.Value = input.Value;
        promo.StartsAt = input.StartsAt;
        promo.EndsAt = input.EndsAt;
        promo.ConditionsJson = string.IsNullOrWhiteSpace(input.ConditionsJson) ? null : input.ConditionsJson;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        var promo = await _db.Promotions.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (promo is null) return false;
        _db.Promotions.Remove(promo);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
