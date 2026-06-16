namespace HtMobile.Domain.Constants;

/// <summary>Khóa cache Redis chuẩn hoá. Dùng để get/set và invalidate nhất quán.</summary>
public static class CacheKeys
{
    public const string CategoryTreePrefix = "catalog:category-tree";

    /// <summary>Giá hiệu lực của 1 variant theo vùng.</summary>
    public static string VariantPrice(long variantId, long regionId) =>
        $"pricing:variant:{variantId}:region:{regionId}";

    /// <summary>Khuyến mãi đang hiệu lực của 1 sản phẩm.</summary>
    public static string ProductPromotions(long productId) =>
        $"pricing:product:{productId}:promotions";

    public static string SearchSuggest(string query) =>
        $"search:suggest:{query.Trim().ToLowerInvariant()}";
}
