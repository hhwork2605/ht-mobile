using HtMobile.Domain.Common;
using HtMobile.Domain.Entities.Catalog;

namespace HtMobile.Domain.Entities.Sales;

/// <summary>Combo "mua kèm phụ kiện" cho 1 sản phẩm chính. SPEC §4.2, §5.</summary>
public class Bundle : BaseAuditableEntity
{
    public long MainProductId { get; set; }

    public Product MainProduct { get; set; } = null!;
    public ICollection<BundleItem> Items { get; set; } = new List<BundleItem>();
}
