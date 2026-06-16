using HtMobile.Domain.Common;
using HtMobile.Domain.Entities.Catalog;

namespace HtMobile.Domain.Entities.Reviews;

/// <summary>Đánh giá sản phẩm (rating sao + nội dung). SPEC §5.</summary>
public class Review : BaseAuditableEntity
{
    public long VariantId { get; set; }
    public long? CustomerId { get; set; }
    public int Rating { get; set; }   // 1..5
    public string? Content { get; set; }

    public ProductVariant Variant { get; set; } = null!;
}
