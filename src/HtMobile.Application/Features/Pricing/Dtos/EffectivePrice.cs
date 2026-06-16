namespace HtMobile.Application.Features.Pricing.Dtos;

/// <summary>Giá hiệu lực của 1 variant theo vùng, sau khi áp khuyến mãi.</summary>
public record EffectivePrice
{
    public long VariantId { get; init; }
    public long RegionId { get; init; }

    /// <summary>Giá niêm yết tại vùng (trước khuyến mãi).</summary>
    public decimal ListPrice { get; init; }

    /// <summary>Giá gạch ngang (giá gốc cao hơn), nếu có.</summary>
    public decimal? CompareAtPrice { get; init; }

    /// <summary>Giá phải trả sau khi áp khuyến mãi.</summary>
    public decimal FinalPrice { get; init; }

    /// <summary>% giảm so với mốc cao nhất (CompareAt hoặc ListPrice).</summary>
    public int DiscountPercent { get; init; }

    public string? AppliedPromotionName { get; init; }
}
