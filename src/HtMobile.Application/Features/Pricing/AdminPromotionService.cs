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
                Code = p.Code,
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
                Id = p.Id, Name = p.Name, Code = p.Code, Type = p.Type, Value = p.Value,
                StartsAt = p.StartsAt, EndsAt = p.EndsAt, ConditionsJson = p.ConditionsJson,
            })
            .FirstOrDefaultAsync(ct);

    public async Task<(PromotionWriteResult Result, long Id)> CreateAsync(PromotionInput input, CancellationToken ct = default)
    {
        var code = NormalizeCode(input.Code);
        if (code is not null && await _db.Promotions.AnyAsync(p => p.Code == code, ct))
            return (PromotionWriteResult.CodeExists, 0);

        var promo = new Promotion
        {
            Name = input.Name.Trim(),
            Code = code,
            Type = input.Type,
            Value = input.Value,
            StartsAt = input.StartsAt,
            EndsAt = input.EndsAt,
            ConditionsJson = string.IsNullOrWhiteSpace(input.ConditionsJson) ? null : input.ConditionsJson,
        };
        _db.Promotions.Add(promo);
        await _db.SaveChangesAsync(ct);
        return (PromotionWriteResult.Ok, promo.Id);
    }

    public async Task<PromotionWriteResult> UpdateAsync(long id, PromotionInput input, CancellationToken ct = default)
    {
        var promo = await _db.Promotions.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (promo is null) return PromotionWriteResult.NotFound;

        var code = NormalizeCode(input.Code);
        if (code is not null && await _db.Promotions.AnyAsync(p => p.Code == code && p.Id != id, ct))
            return PromotionWriteResult.CodeExists;

        promo.Name = input.Name.Trim();
        promo.Code = code;
        promo.Type = input.Type;
        promo.Value = input.Value;
        promo.StartsAt = input.StartsAt;
        promo.EndsAt = input.EndsAt;
        promo.ConditionsJson = string.IsNullOrWhiteSpace(input.ConditionsJson) ? null : input.ConditionsJson;
        await _db.SaveChangesAsync(ct);
        return PromotionWriteResult.Ok;
    }

    /// <summary>Chuẩn hoá mã voucher: trim + UPPER; rỗng → null (KM tự động).</summary>
    private static string? NormalizeCode(string? code)
    {
        var c = code?.Trim().ToUpperInvariant();
        return string.IsNullOrEmpty(c) ? null : c;
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
