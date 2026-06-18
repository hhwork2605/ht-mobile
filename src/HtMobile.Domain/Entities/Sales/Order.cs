using HtMobile.Domain.Common;
using HtMobile.Domain.Entities.Customers;
using HtMobile.Domain.Enums;

namespace HtMobile.Domain.Entities.Sales;

public class Order : BaseAuditableEntity
{
    /// <summary>Khách đặt hàng (FK → <see cref="Customers.Customer"/>); <c>null</c> = khách vãng lai (guest checkout, P2-02).</summary>
    public long? CustomerId { get; set; }

    /// <summary>Khách hàng đặt đơn (null nếu guest).</summary>
    public Customer? Customer { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Total { get; set; }
    public string? PaymentMethod { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public Shipment? Shipment { get; set; }
}
