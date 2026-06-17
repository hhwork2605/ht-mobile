using HtMobile.Domain.Common;
using HtMobile.Domain.Entities.Catalog;

namespace HtMobile.Domain.Entities.Reviews;

/// <summary>Đánh giá sản phẩm (rating sao + nội dung). SPEC §5.</summary>
public class Review : BaseAuditableEntity
{
    /// <summary>Sản phẩm/biến thể được đánh giá = Product. (Trước đây là VariantId → ProductVariant.)</summary>
    public long ProductId { get; set; }
    public long? CustomerId { get; set; }
    public int Rating { get; set; }   // 1..5
    public string? Content { get; set; }

    public Product Product { get; set; } = null!;
}
