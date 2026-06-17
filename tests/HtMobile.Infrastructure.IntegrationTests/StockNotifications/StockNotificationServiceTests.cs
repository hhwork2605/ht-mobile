using FluentAssertions;
using HtMobile.Application.Features.StockNotifications;
using HtMobile.Domain.Entities.Catalog;
using HtMobile.Domain.Enums;
using HtMobile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Infrastructure.IntegrationTests.StockNotifications;

/// <summary>StockNotificationService trên EF InMemory: chỉ OutOfStock mới đăng ký được; dedupe; còn hàng → từ chối.</summary>
public class StockNotificationServiceTests
{
    // variant 10 = OutOfStock, 11 = Active
    private static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("stock-" + Guid.NewGuid().ToString("N"))
            .Options;
        var db = new AppDbContext(options);
        db.Products.Add(new Product { Id = 1, CategoryId = 1, Name = "iPhone 17", Slug = "iphone-17" });
        db.Products.AddRange(
            new Product { Id = 10, ProductParentId = 1, CategoryId = 1, Name = "iPhone 17", Sku = "A", Slug = "v-oos", BasePrice = 1m, Status = ProductStatus.OutOfStock },
            new Product { Id = 11, ProductParentId = 1, CategoryId = 1, Name = "iPhone 17", Sku = "B", Slug = "v-active", BasePrice = 1m, Status = ProductStatus.Active });
        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task Subscribe_creates_notification_for_out_of_stock()
    {
        using var db = NewDb();
        var svc = new StockNotificationService(db);

        var r = await svc.SubscribeAsync(10, "user@example.com");

        r.Should().Be(StockNotifyResult.Subscribed);
        (await db.StockNotifications.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Duplicate_subscription_is_idempotent()
    {
        using var db = NewDb();
        var svc = new StockNotificationService(db);
        await svc.SubscribeAsync(10, "user@example.com");

        var r = await svc.SubscribeAsync(10, "user@example.com");

        r.Should().Be(StockNotifyResult.AlreadySubscribed);
        (await db.StockNotifications.CountAsync()).Should().Be(1);   // không tạo trùng
    }

    [Fact]
    public async Task In_stock_variant_is_rejected()
    {
        using var db = NewDb();
        var r = await new StockNotificationService(db).SubscribeAsync(11, "user@example.com");

        r.Should().Be(StockNotifyResult.StillInStock);
        (await db.StockNotifications.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Invalid_contact_creates_nothing()
    {
        using var db = NewDb();
        var r = await new StockNotificationService(db).SubscribeAsync(10, "rác");

        r.Should().Be(StockNotifyResult.InvalidContact);
        (await db.StockNotifications.CountAsync()).Should().Be(0);
    }
}
