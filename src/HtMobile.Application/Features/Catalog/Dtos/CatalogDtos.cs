namespace HtMobile.Application.Features.Catalog.Dtos;

/// <summary>Mục danh mục cho menu/điều hướng.</summary>
public record CategoryDto(long Id, string Name, string Slug, int SortOrder);

/// <summary>Thẻ sản phẩm trên lưới (SPEC §4.1): ảnh, badge giảm %, badge "Mới", giá gạch + giá bán.</summary>
public record ProductCardDto
{
    public long ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Tagline { get; init; }
    public string ProductSlug { get; init; } = string.Empty;
    public long VariantId { get; init; }
    public string VariantSlug { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public decimal FinalPrice { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public int DiscountPercent { get; init; }
    public bool IsNew { get; init; }
}

/// <summary>Slide banner cho carousel trang chủ.</summary>
public record BannerDto(string? Eyebrow, string Title, string? Subtitle, string? ImageUrl, string? LinkUrl, string? CtaText);

/// <summary>Dữ liệu trang chủ: banner slider + tile/nav danh mục + lưới sản phẩm nổi bật.</summary>
public record HomePageDto
{
    public IReadOnlyList<BannerDto> Banners { get; init; } = Array.Empty<BannerDto>();
    public IReadOnlyList<CategoryDto> Categories { get; init; } = Array.Empty<CategoryDto>();
    public IReadOnlyList<ProductCardDto> Featured { get; init; } = Array.Empty<ProductCardDto>();
}

/// <summary>Trang danh mục: thông tin danh mục + lưới sản phẩm + block SEO.</summary>
public record CategoryPageDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? SeoContent { get; init; }
    public IReadOnlyList<ProductCardDto> Products { get; init; } = Array.Empty<ProductCardDto>();
}
