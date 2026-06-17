using FluentAssertions;
using HtMobile.Application.Features.Checkout;
using HtMobile.Domain.Enums;
using Xunit;

namespace HtMobile.Application.UnitTests.Checkout;

public class OrderFactoryTests
{
    private static readonly OrderLineInput[] TwoLines =
    {
        new(VariantId: 10, UnitPrice: 1_000_000m, Quantity: 2),  // 2.000.000
        new(VariantId: 11, UnitPrice: 500_000m, Quantity: 1),    //   500.000
    };

    [Fact]
    public void Empty_cart_creates_no_order()
    {
        OrderFactory.Create(Array.Empty<OrderLineInput>(), customerId: 1, "addr", "COD")
            .Should().BeNull();
    }

    [Fact]
    public void Builds_order_with_total_items_shipment_and_cod_payment()
    {
        var order = OrderFactory.Create(TwoLines, customerId: 42, "Nguyen · 0900 · Hà Nội", "COD");

        order.Should().NotBeNull();
        order!.CustomerId.Should().Be(42);
        order.Status.Should().Be(OrderStatus.Pending);
        order.Total.Should().Be(2_500_000m);
        order.PaymentMethod.Should().Be("COD");

        order.Items.Should().HaveCount(2);
        order.Items.Should().ContainSingle(i => i.VariantId == 10 && i.Quantity == 2 && i.UnitPrice == 1_000_000m);

        order.Shipment.Should().NotBeNull();
        order.Shipment!.Address.Should().Be("Nguyen · 0900 · Hà Nội");

        order.Payments.Should().ContainSingle();
        order.Payments.Single().Amount.Should().Be(2_500_000m);
        order.Payments.Single().Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void Guest_order_has_null_customer()
    {
        var order = OrderFactory.Create(TwoLines, customerId: null, "addr", "COD");
        order!.CustomerId.Should().BeNull();
    }
}
