using HtMobile.Domain.Common;
using HtMobile.Domain.Entities.Catalog;

namespace HtMobile.Domain.Entities.Sales;

public class OrderItem : BaseEntity
{
    public long OrderId { get; set; }
    /// <summary>Biến thể đã mua = Product con. (Trước đây là VariantId → ProductVariant.)</summary>
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
