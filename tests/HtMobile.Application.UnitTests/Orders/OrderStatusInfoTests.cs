using FluentAssertions;
using HtMobile.Application.Features.Orders;
using HtMobile.Domain.Enums;
using Xunit;

namespace HtMobile.Application.UnitTests.Orders;

public class OrderStatusInfoTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, 0)]
    [InlineData(OrderStatus.Confirmed, 1)]
    [InlineData(OrderStatus.Processing, 1)]
    [InlineData(OrderStatus.Shipped, 2)]
    [InlineData(OrderStatus.Delivered, 3)]
    public void Active_statuses_map_to_step(OrderStatus status, int expectedStep)
    {
        var view = OrderStatusInfo.Describe(status);

        view.StepIndex.Should().Be(expectedStep);
        view.IsCancelled.Should().BeFalse();
        view.Label.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Refunded)]
    public void Cancelled_or_refunded_has_no_step(OrderStatus status)
    {
        var view = OrderStatusInfo.Describe(status);

        view.StepIndex.Should().Be(-1);
        view.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public void Has_four_steps()
    {
        OrderStatusInfo.Steps.Should().HaveCount(4);
    }
}
