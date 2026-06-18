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

    /// <summary>Tổng tiền hàng (trước giảm giá mã).</summary>
    public decimal Subtotal { get; set; }

    /// <summary>Số tiền giảm từ mã giảm giá (0 nếu không có).</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Mã giảm giá đã áp (null nếu không có).</summary>
    public string? CouponCode { get; set; }

    /// <summary>Tổng phải trả = Subtotal − DiscountAmount (+ phí ship).</summary>
    public decimal Total { get; set; }
    public string? PaymentMethod { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public Shipment? Shipment { get; set; }
}
