using FluentAssertions;
using HtMobile.Application.Features.Orders;
using HtMobile.Application.Features.Orders.Dtos;
using HtMobile.Domain.Entities.Sales;
using HtMobile.Domain.Enums;
using HtMobile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Infrastructure.IntegrationTests.Orders;

/// <summary>AdminOrderService trên EF InMemory: đổi trạng thái chỉ theo luồng hợp lệ; NotFound.</summary>
public class AdminOrderServiceTests
{
    private static AppDbContext NewDb(out long orderId)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("adminorder-" + Guid.NewGuid().ToString("N"))
            .Options;
        var db = new AppDbContext(options);
        var order = new Order { CustomerId = 1, Status = OrderStatus.Pending, Total = 1_000_000m, PaymentMethod = "COD" };
        db.Orders.Add(order);
        db.SaveChanges();
        orderId = order.Id;
        return db;
    }

    [Fact]
    public async Task Valid_transition_is_saved()
    {
        var db = NewDb(out var id);
        var svc = new AdminOrderService(db);

        var r = await svc.ChangeStatusAsync(id, OrderStatus.Confirmed);

        r.Should().Be(ChangeStatusResult.Ok);
        (await db.Orders.FindAsync(id))!.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task Invalid_transition_is_rejected_and_not_saved()
    {
        var db = NewDb(out var id);
        var svc = new AdminOrderService(db);

        // Pending → Delivered là nhảy cóc → từ chối.
        var r = await svc.ChangeStatusAsync(id, OrderStatus.Delivered);

        r.Should().Be(ChangeStatusResult.InvalidTransition);
        (await db.Orders.FindAsync(id))!.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public async Task Unknown_order_is_not_found()
    {
        var db = NewDb(out _);
        var r = await new AdminOrderService(db).ChangeStatusAsync(999999, OrderStatus.Confirmed);
        r.Should().Be(ChangeStatusResult.NotFound);
    }
}
