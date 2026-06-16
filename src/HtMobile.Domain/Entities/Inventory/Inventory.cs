using HtMobile.Domain.Common;
using HtMobile.Domain.Entities.Catalog;

namespace HtMobile.Domain.Entities.Inventory;

/// <summary>Tồn kho của 1 variant tại 1 cửa hàng ("cửa hàng có sẵn"). SPEC §4.2.</summary>
public class Inventory : BaseAuditableEntity
{
    public long VariantId { get; set; }
    public long StoreId { get; set; }
    public int Quantity { get; set; }

    public ProductVariant Variant { get; set; } = null!;
    public Store Store { get; set; } = null!;
}
