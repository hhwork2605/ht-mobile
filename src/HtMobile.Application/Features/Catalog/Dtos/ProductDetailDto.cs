using HtMobile.Application.Features.Bundles.Dtos;
using HtMobile.Application.Features.Pricing.Dtos;

namespace HtMobile.Application.Features.Catalog.Dtos;

/// <summary>Dữ liệu trang chi tiết sản phẩm (PDP) — SPEC §5.</summary>
public record ProductDetailDto
{
    public long ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ProductSlug { get; init; } = string.Empty;
    public string? Tagline { get; init; }
    public string? Brand { get; init; }
    public string? Description { get; init; }
    public string? SpecsJson { get; init; }

    /// <summary>Tên + slug danh mục cha (cho breadcrumb + JSON-LD).</summary>
    public string CategoryName { get; init; } = string.Empty;
    public string CategorySlug { get; init; } = string.Empty;

    public long SelectedVariantId { get; init; }

    /// <summary>Slug chuẩn (canonical) gom mọi variant về 1 URL để tránh trùng nội dung SEO — dùng variant mặc định.</summary>
    public string CanonicalSlug { get; init; } = string.Empty;

    /// <summary>Nhãn biến thể đang chọn ghép từ thuộc tính (vd "256GB · Đen"); rỗng nếu không có.</summary>
    public string? SelectedVariantLabel { get; init; }
    public string SelectedSku { get; init; } = string.Empty;

    /// <summary>Còn bán (variant Active) → JSON-LD availability InStock.</summary>
    public bool InStock { get; init; }

    public EffectivePrice Price { get; init; } = null!;

    public IReadOnlyList<VariantOptionDto> Variants { get; init; } = Array.Empty<VariantOptionDto>();
    public IReadOnlyList<string> Images { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> YoutubeUrls { get; init; } = Array.Empty<string>();

    /// <summary>Khuyến mãi đang hiệu lực (khối "Ưu đãi").</summary>
    public IReadOnlyList<OfferDto> Offers { get; init; } = Array.Empty<OfferDto>();

    /// <summary>Ưu đãi thanh toán theo ngân hàng (carousel).</summary>
    public IReadOnlyList<PaymentOfferDto> PaymentOffers { get; init; } = Array.Empty<PaymentOfferDto>();

    /// <summary>Combo "mua kèm phụ kiện" (P3-01); null nếu sản phẩm không có bundle.</summary>
    public BundleView? Bundle { get; init; }

    /// <summary>Tổng số đánh giá của các variant thuộc sản phẩm.</summary>
    public int ReviewCount { get; init; }

    /// <summary>Điểm trung bình (1..5); null nếu chưa có đánh giá.</summary>
    public double? AverageRating { get; init; }
}

/// <summary>Một biến thể (Product con) để chọn — nhãn ghép từ thuộc tính; mỗi cái có slug/URL riêng.</summary>
public record VariantOptionDto(
    long Id,
    string Slug,
    string Label,
    string Sku,
    bool IsSelected);

/// <summary>Khuyến mãi hiệu lực hiển thị ở khối "Ưu đãi".</summary>
public record OfferDto(string Name, DateTime EndsAt);

/// <summary>Ưu đãi thanh toán theo ngân hàng/ví.</summary>
public record PaymentOfferDto(string Bank, string Title, string? Description, DateTime EndsAt);
