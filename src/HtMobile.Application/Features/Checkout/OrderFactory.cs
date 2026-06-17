using HtMobile.Domain.Entities.Sales;

namespace HtMobile.Application.Features.Checkout;

/// <summary>Một dòng đơn đã có giá hiệu lực (lấy qua IPricingService trước khi gọi factory).</summary>
public readonly record struct OrderLineInput(long ProductId, decimal UnitPrice, int Quantity);

/// <summary>
/// Dựng <see cref="Order"/> (THUẦN, không DB) từ các dòng đã định giá + thông tin giao + phương thức thanh toán.
/// Tổng tiền tính tại đây; trả <c>null</c> nếu không có dòng nào (không tạo đơn rỗng).
/// </summary>
public static class OrderFactory
{
    public static Order? Create(
        IReadOnlyList<OrderLineInput> lines,
        long? customerId,
        string shippingAddress,
        string paymentMethod)
    {
        if (lines is null || lines.Count == 0) return null;

        var order = new Order
        {
            CustomerId = customerId,
            Status = Domain.Enums.OrderStatus.Pending,
            PaymentMethod = paymentMethod
        };

        decimal total = 0m;
        foreach (var l in lines)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            });
            total += l.UnitPrice * l.Quantity;
        }

        order.Total = total;
        order.Shipment = new Shipment
        {
            Address = shippingAddress,
            Status = Domain.Enums.ShipmentStatus.Pending
        };
        order.Payments.Add(new Payment
        {
            Provider = paymentMethod,
            Status = Domain.Enums.PaymentStatus.Pending,
            Amount = total
        });

        return order;
    }
}
