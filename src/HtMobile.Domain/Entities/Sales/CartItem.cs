using HtMobile.Domain.Common;
using HtMobile.Domain.Entities.Catalog;

namespace HtMobile.Domain.Entities.Sales;

public class CartItem : BaseEntity
{
    public long CartId { get; set; }
    /// <summary>Biến thể trong giỏ = Product con. (Trước đây là VariantId → ProductVariant.)</summary>
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    /// <summary>Giá cố định cho dòng (vd giá mua kèm bundle, P3-01). null = dùng giá hiệu lực qua IPricingService.</summary>
    public decimal? UnitPriceOverride { get; set; }

    public Cart Cart { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
