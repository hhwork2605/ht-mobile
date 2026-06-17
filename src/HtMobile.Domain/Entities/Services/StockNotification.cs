using HtMobile.Domain.Common;
using HtMobile.Domain.Entities.Catalog;

namespace HtMobile.Domain.Entities.Services;

/// <summary>"Theo dõi để biết khi có hàng" cho biến thể (Product con) hết hàng. SPEC §4.1, UC-05.</summary>
public class StockNotification : BaseAuditableEntity
{
    /// <summary>Biến thể theo dõi = Product con. (Trước đây là VariantId → ProductVariant.)</summary>
    public long ProductId { get; set; }
    public string Contact { get; set; } = string.Empty;   // email hoặc SĐT
    public bool Notified { get; set; }

    public Product Product { get; set; } = null!;
}
