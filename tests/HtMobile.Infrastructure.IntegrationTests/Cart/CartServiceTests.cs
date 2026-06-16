using FluentAssertions;
using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Cart;
using HtMobile.Application.Features.Pricing.Dtos;
using HtMobile.Domain.Entities.Catalog;
using HtMobile.Domain.Enums;
using HtMobile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Infrastructure.IntegrationTests.Cart;

/// <summary>
/// Test CartService trên EF InMemory (logic giỏ DB-bound: gộp, qty→0, ownership, merge, lọc Status).
/// Đơn giá lấy qua stub IPricingService (cố định 1.000.000đ/biến thể).
/// </summary>
public class CartServiceTests
{
    private const decimal Unit = 1_000_000m;

    // variant 10 = Active, 11 = Active (sp khác), 99 = Discontinued
    private static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("cart-" + Guid.NewGuid().ToString("N"))
            .Options;
        var db = new AppDbContext(options);

        var product = new Product { Id = 1, CategoryId = 1, Name = "iPhone 17", Slug = "iphone-17" };
        product.Images.Add(new ProductImage { Id = 1, ProductId = 1, Url = "/img.png", SortOrder = 0 });
        db.Products.Add(product);
        db.ProductVariants.AddRange(
            new ProductVariant { Id = 10, ProductId = 1, Sku = "A", Slug = "iphone-17-256", Storage = "256GB", Color = "Đen", BasePrice = Unit, Status = VariantStatus.Active },
            new ProductVariant { Id = 11, ProductId = 1, Sku = "B", Slug = "iphone-17-512", Storage = "512GB", Color = "Trắng", BasePrice = Unit, Status = VariantStatus.Active },
            new ProductVariant { Id = 99, ProductId = 1, Sku = "C", Slug = "iphone-17-old", Storage = "128GB", Color = "Xám", BasePrice = Unit, Status = VariantStatus.Discontinued });
        db.SaveChanges();
        return db;
    }

    private static CartService Service(AppDbContext db) => new(db, new StubPricing());

    private static readonly CartOwner Guest = new(null, "sess-guest");
    private static readonly CartOwner User = new(42, null);

    [Fact]
    public async Task Add_then_add_same_variant_merges_quantity()
    {
        using var db = NewDb();
        var svc = Service(db);

        await svc.AddItemAsync(Guest, variantId: 10, quantity: 1);
        var count = await svc.AddItemAsync(Guest, variantId: 10, quantity: 2);

        count.Should().Be(3);
        var cart = await svc.GetCartAsync(Guest);
        cart.Items.Should().HaveCount(1);
        cart.Items[0].Quantity.Should().Be(3);
        cart.Subtotal.Should().Be(3 * Unit);
    }

    [Fact]
    public async Task Add_discontinued_variant_is_rejected()
    {
        using var db = NewDb();
        var svc = Service(db);

        var count = await svc.AddItemAsync(Guest, variantId: 99, quantity: 1);

        count.Should().Be(0);
        (await svc.GetCartAsync(Guest)).IsEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task Decrement_to_zero_removes_line()
    {
        using var db = NewDb();
        var svc = Service(db);
        await svc.AddItemAsync(Guest, 10, 1);
        var item = (await svc.GetCartAsync(Guest)).Items[0];

        await svc.UpdateQuantityAsync(Guest, item.Id, -1);

        (await svc.GetCartAsync(Guest)).IsEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task User_cannot_remove_item_of_another_cart()
    {
        using var db = NewDb();
        var svc = Service(db);
        await svc.AddItemAsync(Guest, 10, 1);
        var guestItem = (await svc.GetCartAsync(Guest)).Items[0];

        // User khác cố xoá item của giỏ guest qua id → không được phép, giỏ guest còn nguyên.
        await svc.RemoveItemAsync(User, guestItem.Id);

        (await svc.GetCartAsync(Guest)).Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Merge_combines_guest_cart_into_user_cart()
    {
        using var db = NewDb();
        var svc = Service(db);
        await svc.AddItemAsync(Guest, 10, 1);
        await svc.AddItemAsync(Guest, 11, 2);
        await svc.AddItemAsync(User, 10, 1);   // user đã có variant 10

        await svc.MergeAsync("sess-guest", 42);

        var userCart = await svc.GetCartAsync(User);
        userCart.Items.Should().HaveCount(2);
        userCart.Items.Single(i => i.VariantId == 10).Quantity.Should().Be(2);  // 1 + 1
        userCart.Items.Single(i => i.VariantId == 11).Quantity.Should().Be(2);
        // giỏ guest đã bị xoá
        (await svc.GetCartAsync(Guest)).IsEmpty.Should().BeTrue();
    }

    private sealed class StubPricing : IPricingService
    {
        public Task<EffectivePrice> GetEffectivePriceAsync(long variantId, CancellationToken ct = default)
            => Task.FromResult(new EffectivePrice { VariantId = variantId, ListPrice = Unit, FinalPrice = Unit });

        public Task InvalidateAsync(long variantId, CancellationToken ct = default) => Task.CompletedTask;
    }
}
