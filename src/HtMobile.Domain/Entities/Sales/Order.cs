using HtMobile.Domain.Common;
using HtMobile.Domain.Enums;

namespace HtMobile.Domain.Entities.Sales;

public class Order : BaseAuditableEntity
{
    public long CustomerId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Total { get; set; }
    public string? PaymentMethod { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public Shipment? Shipment { get; set; }
}
