using HtMobile.Domain.Common;

namespace HtMobile.Domain.Entities.Catalog;

public class ProductImage : BaseEntity
{
    public long ProductId { get; set; }
    public long? VariantId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Product Product { get; set; } = null!;
    public ProductVariant? Variant { get; set; }
}
