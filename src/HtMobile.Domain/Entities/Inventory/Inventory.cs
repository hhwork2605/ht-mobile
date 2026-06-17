using HtMobile.Domain.Common;
using HtMobile.Domain.Entities.Catalog;

namespace HtMobile.Domain.Entities.Inventory;

/// <summary>Tồn kho của 1 biến thể (Product con) tại 1 cửa hàng ("cửa hàng có sẵn"). SPEC §4.2.</summary>
public class Inventory : BaseAuditableEntity
{
    /// <summary>Biến thể = Product con. (Trước đây là VariantId → ProductVariant.)</summary>
    public long ProductId { get; set; }
    public long StoreId { get; set; }
    public int Quantity { get; set; }

    public Product Product { get; set; } = null!;
    public Store Store { get; set; } = null!;
}
