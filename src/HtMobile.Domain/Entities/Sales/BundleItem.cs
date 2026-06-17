using HtMobile.Domain.Common;
using HtMobile.Domain.Entities.Catalog;

namespace HtMobile.Domain.Entities.Sales;

public class BundleItem : BaseEntity
{
    public long BundleId { get; set; }
    /// <summary>Phụ kiện mua kèm = Product con. (Trước đây là AccessoryVariantId → ProductVariant.)</summary>
    public long AccessoryProductId { get; set; }
    public decimal BundlePrice { get; set; }   // giá ưu đãi khi mua kèm

    public Bundle Bundle { get; set; } = null!;
    public Product AccessoryProduct { get; set; } = null!;
}
