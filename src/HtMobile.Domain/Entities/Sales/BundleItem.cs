using HtMobile.Domain.Common;
using HtMobile.Domain.Entities.Catalog;

namespace HtMobile.Domain.Entities.Sales;

public class BundleItem : BaseEntity
{
    public long BundleId { get; set; }
    public long AccessoryVariantId { get; set; }
    public decimal BundlePrice { get; set; }   // giá ưu đãi khi mua kèm

    public Bundle Bundle { get; set; } = null!;
    public ProductVariant AccessoryVariant { get; set; } = null!;
}
