namespace HtMobile.Application.Features.Bundles.Dtos;

/// <summary>Một phụ kiện gợi ý mua kèm (giá niêm yết vs giá mua kèm).</summary>
public record BundleAccessoryView
{
    public long VariantId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string VariantText { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public string VariantSlug { get; init; } = string.Empty;
    public decimal ListedPrice { get; init; }
    public decimal BundlePrice { get; init; }
    public decimal Saving { get; init; }
}

/// <summary>Khối "mua kèm phụ kiện" cho PDP.</summary>
public record BundleView
{
    public IReadOnlyList<BundleAccessoryView> Accessories { get; init; } = Array.Empty<BundleAccessoryView>();
    public decimal TotalListed { get; init; }
    public decimal TotalBundle { get; init; }
    public decimal TotalSaving { get; init; }
}
