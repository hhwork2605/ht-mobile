using HtMobile.Domain.Common;
using HtMobile.Domain.Entities.Catalog;

namespace HtMobile.Domain.Entities.Services;

/// <summary>"Theo dõi để biết khi có hàng" cho variant hết hàng. SPEC §4.1, UC-05.</summary>
public class StockNotification : BaseAuditableEntity
{
    public long VariantId { get; set; }
    public string Contact { get; set; } = string.Empty;   // email hoặc SĐT
    public bool Notified { get; set; }

    public ProductVariant Variant { get; set; } = null!;
}
