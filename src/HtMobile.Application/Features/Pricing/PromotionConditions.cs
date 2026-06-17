using System.Text.Json;

namespace HtMobile.Application.Features.Pricing;

/// <summary>Ngữ cảnh định giá 1 variant để xét điều kiện khuyến mãi.</summary>
public readonly record struct PricingContext(long VariantId, long ProductId, long CategoryId);

/// <summary>
/// Parse + khớp <c>Promotion.ConditionsJson</c> (THUẦN, dễ test). Schema (mọi trường optional):
/// <c>{ "categoryIds":[], "productIds":[], "variantIds":[] }</c>.
/// null/rỗng = áp toàn bộ; có danh sách thì variant phải khớp ≥1 trong các danh sách được khai.
/// </summary>
public static class PromotionConditions
{
    private static readonly JsonSerializerOptions Opts = new(JsonSerializerDefaults.Web);

    private sealed class Conditions
    {
        public long[]? CategoryIds { get; set; }
        public long[]? ProductIds { get; set; }
        public long[]? VariantIds { get; set; }
    }

    public static bool Matches(string? conditionsJson, PricingContext ctx)
    {
        if (string.IsNullOrWhiteSpace(conditionsJson)) return true;

        Conditions? c;
        try { c = JsonSerializer.Deserialize<Conditions>(conditionsJson, Opts); }
        catch (JsonException) { return false; }   // JSON hỏng → không áp (an toàn)
        if (c is null) return true;

        var hasCat = c.CategoryIds is { Length: > 0 };
        var hasProd = c.ProductIds is { Length: > 0 };
        var hasVar = c.VariantIds is { Length: > 0 };
        if (!hasCat && !hasProd && !hasVar) return true;   // {} → áp toàn bộ

        // OR: khớp ≥1 danh sách được khai.
        if (hasCat && c.CategoryIds!.Contains(ctx.CategoryId)) return true;
        if (hasProd && c.ProductIds!.Contains(ctx.ProductId)) return true;
        if (hasVar && c.VariantIds!.Contains(ctx.VariantId)) return true;
        return false;
    }
}
