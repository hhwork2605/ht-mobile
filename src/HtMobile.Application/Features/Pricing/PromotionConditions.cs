using System.Text.Json;

namespace HtMobile.Application.Features.Pricing;

/// <summary>
/// Ngữ cảnh định giá 1 biến thể (Product con) để xét điều kiện khuyến mãi.
/// ProductId = Id biến thể con (bán); ParentProductId = model cha (null nếu đứng độc lập).
/// </summary>
public readonly record struct PricingContext(long ProductId, long? ParentProductId, long CategoryId);

/// <summary>
/// Parse + khớp <c>Promotion.ConditionsJson</c> (THUẦN, dễ test). Schema (mọi trường optional):
/// <c>{ "categoryIds":[], "productIds":[], "variantIds":[] }</c>.
/// null/rỗng = áp toàn bộ; có danh sách thì phải khớp ≥1. Ngữ nghĩa mới: <c>variantIds</c> khớp Id biến thể con
/// (<see cref="PricingContext.ProductId"/>), <c>productIds</c> khớp model cha (<see cref="PricingContext.ParentProductId"/>).
/// </summary>
public static class PromotionConditions
{
    private static readonly JsonSerializerOptions Opts = new(JsonSerializerDefaults.Web);

    private sealed class Conditions
    {
        public long[]? CategoryIds { get; set; }
        public long[]? ProductIds { get; set; }
        public long[]? VariantIds { get; set; }

        /// <summary>Ngưỡng đơn tối thiểu (đồng) để áp voucher; null = không yêu cầu.</summary>
        public decimal? MinOrder { get; set; }
    }

    /// <summary>Đọc ngưỡng đơn tối thiểu từ ConditionsJson (dùng cho voucher). null nếu không có / JSON hỏng.</summary>
    public static decimal? GetMinOrder(string? conditionsJson)
    {
        if (string.IsNullOrWhiteSpace(conditionsJson)) return null;
        try { return JsonSerializer.Deserialize<Conditions>(conditionsJson, Opts)?.MinOrder; }
        catch (JsonException) { return null; }
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
        // productIds = model cha; nếu biến thể đứng độc lập (không cha) thì so với chính nó.
        var modelId = ctx.ParentProductId ?? ctx.ProductId;
        if (hasProd && c.ProductIds!.Contains(modelId)) return true;
        // variantIds = Id biến thể con (sản phẩm bán thực sự).
        if (hasVar && c.VariantIds!.Contains(ctx.ProductId)) return true;
        return false;
    }
}
