using FluentAssertions;
using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Cart;
using HtMobile.Application.Features.Checkout;
using HtMobile.Application.Features.Pricing.Dtos;
using HtMobile.Domain.Entities.Catalog;
using HtMobile.Domain.Enums;
using HtMobile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Infrastructure.IntegrationTests.Checkout;

/// <summary>CheckoutService trên EF InMemory: đặt hàng từ giỏ, snapshot giá, xoá giỏ; giỏ rỗng → không tạo đơn.</summary>
public class CheckoutServiceTests
{
    private const decimal Unit = 1_000_000m;
    private static readonly CartOwner Guest = new(null, "sess-guest");

    private static (AppDbContext db, CheckoutService checkout, CartService cart) Build()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("checkout-" + Guid.NewGuid().ToString("N"))
            .Options;
        var db = new AppDbContext(options);

        var product = new Product { Id = 1, CategoryId = 1, Name = "iPhone 17", Slug = "iphone-17" };
        product.Images.Add(new ProductImage { Id = 1, ProductId = 1, Url = "/img.png", SortOrder = 0 });
        db.Products.Add(product);
        db.Products.Add(new Product
        {
            Id = 10, ProductParentId = 1, CategoryId = 1, Name = "iPhone 17", Sku = "A", Slug = "iphone-17-256",
            BasePrice = Unit, Status = ProductStatus.Active
        });
        db.SaveChanges();

        var cart = new CartService(db, new StubPricing(), new HtMobile.Infrastructure.Common.SystemDateTime());
        return (db, new CheckoutService(db, cart), cart);
    }

    [Fact]
    public async Task Empty_cart_places_no_order()
    {
        var (db, checkout, _) = Build();

        var result = await checkout.PlaceOrderAsync(Guest, "Nguyen · 0900 · HN");

        result.Should().BeNull();
        (await db.Orders.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Places_order_snapshots_price_and_clears_cart()
    {
        var (db, checkout, cart) = Build();
        await cart.AddItemAsync(Guest, variantId: 10, quantity: 2);

        var result = await checkout.PlaceOrderAsync(Guest, "Nguyen · 0900 · HN");

        result.Should().NotBeNull();
        result!.Code.Should().StartWith("SD");

        var order = await db.Orders.Include(o => o.Items).Include(o => o.Payments)
            .Include(o => o.Shipment).SingleAsync();
        order.Status.Should().Be(OrderStatus.Pending);
        order.Total.Should().Be(2 * Unit);
        order.CustomerId.Should().BeNull();                 // guest
        order.Items.Single().UnitPrice.Should().Be(Unit);   // snapshot giá hiệu lực
        order.Payments.Single().Amount.Should().Be(2 * Unit);
        order.Shipment!.Address.Should().Contain("Nguyen");

        // Giỏ đã bị xoá
        (await db.Carts.CountAsync()).Should().Be(0);
        (await checkout.GetSummaryAsync(Guest)).IsEmpty.Should().BeTrue();
    }

    private sealed class StubPricing : IPricingService
    {
        public Task<EffectivePrice> GetEffectivePriceAsync(long productId, CancellationToken ct = default)
            => Task.FromResult(new EffectivePrice { ProductId = productId, ListPrice = Unit, FinalPrice = Unit });

        public Task InvalidateAsync(long productId, CancellationToken ct = default) => Task.CompletedTask;
    }
}
