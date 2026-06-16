using HtMobile.Application.Features.Pricing.Dtos;

namespace HtMobile.Application.Features.Catalog.Dtos;

/// <summary>Dữ liệu trang chi tiết sản phẩm (PDP) — SPEC §5.</summary>
public record ProductDetailDto
{
    public long ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ProductSlug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? SpecsJson { get; init; }

    public long SelectedVariantId { get; init; }
    public EffectivePrice Price { get; init; } = null!;

    public IReadOnlyList<VariantOptionDto> Variants { get; init; } = Array.Empty<VariantOptionDto>();
    public IReadOnlyList<string> Images { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> YoutubeUrls { get; init; } = Array.Empty<string>();
}

/// <summary>Một biến thể để chọn (dung lượng × màu) — mỗi cái có slug/URL riêng.</summary>
public record VariantOptionDto(
    long Id,
    string Slug,
    string? Storage,
    string? Color,
    string Sku,
    bool IsSelected);
