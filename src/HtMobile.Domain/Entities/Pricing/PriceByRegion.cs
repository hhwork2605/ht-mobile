using HtMobile.Domain.Common;
using HtMobile.Domain.Entities.Catalog;

namespace HtMobile.Domain.Entities.Pricing;

/// <summary>Giá của 1 variant theo từng vùng (giá bán + giá gạch ngang).</summary>
public class PriceByRegion : BaseAuditableEntity
{
    public long VariantId { get; set; }
    public long RegionId { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }

    public ProductVariant Variant { get; set; } = null!;
    public Region Region { get; set; } = null!;
}
