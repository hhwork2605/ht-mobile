using FluentAssertions;
using HtMobile.Application.Features.Orders;
using HtMobile.Domain.Enums;
using Xunit;

namespace HtMobile.Application.UnitTests.Orders;

public class OrderStatusTransitionTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Processing)]
    [InlineData(OrderStatus.Processing, OrderStatus.Shipped)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Refunded)]
    public void Valid_transitions_allowed(OrderStatus from, OrderStatus to)
    {
        OrderStatusTransition.CanTransition(from, to).Should().BeTrue();
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Shipped)]     // nhảy cóc
    [InlineData(OrderStatus.Delivered, OrderStatus.Pending)]   // lùi
    [InlineData(OrderStatus.Shipped, OrderStatus.Cancelled)]   // đã giao không huỷ
    [InlineData(OrderStatus.Confirmed, OrderStatus.Confirmed)] // giữ nguyên
    public void Invalid_transitions_rejected(OrderStatus from, OrderStatus to)
    {
        OrderStatusTransition.CanTransition(from, to).Should().BeFalse();
    }

    [Theory]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Refunded)]
    public void Terminal_states_have_no_next(OrderStatus from)
    {
        OrderStatusTransition.AllowedNext(from).Should().BeEmpty();
        OrderStatusTransition.CanTransition(from, OrderStatus.Pending).Should().BeFalse();
    }

    [Fact]
    public void Pending_allows_confirm_and_cancel()
    {
        OrderStatusTransition.AllowedNext(OrderStatus.Pending)
            .Should().BeEquivalentTo(new[] { OrderStatus.Confirmed, OrderStatus.Cancelled });
    }
}
