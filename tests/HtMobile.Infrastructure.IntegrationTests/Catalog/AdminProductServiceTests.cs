using FluentAssertions;
using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Catalog;
using HtMobile.Application.Features.Catalog.Dtos;
using HtMobile.Domain.Entities.Catalog;
using HtMobile.Domain.Enums;
using HtMobile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Infrastructure.IntegrationTests.Catalog;

/// <summary>
/// AdminProductService trên EF InMemory (mô hình mới ADR 0003): model = Product cha, biến thể = Product con;
/// slug auto-gen + unique, SKU unique, toggle, thêm biến thể, thuộc tính Dung lượng/Màu vào ProductAttribute.
/// </summary>
public class AdminProductServiceTests
{
    private sealed class FixedClock : IDateTime
    {
        public DateTime Now => new(2026, 1, 1, 0, 0, 0);
    }

    private sealed class NoopPricing : HtMobile.Application.Common.Interfaces.IPricingService
    {
        public Task<HtMobile.Application.Features.Pricing.Dtos.EffectivePrice> GetEffectivePriceAsync(long productId, CancellationToken ct = default)
            => Task.FromResult(new HtMobile.Application.Features.Pricing.Dtos.EffectivePrice { ProductId = productId });
        public Task InvalidateAsync(long productId, CancellationToken ct = default) => Task.CompletedTask;
    }

    private static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("adminproduct-" + Guid.NewGuid().ToString("N"))
            .Options;
        var db = new AppDbContext(options);
        db.Categories.Add(new Category { Name = "iPhone", Slug = "iphone", SortOrder = 1 });
        db.SaveChanges();
        return db;
    }

    private static AdminProductService NewService(AppDbContext db) => new(db, new FixedClock(), new NoopPricing());

    private static ProductInput P(string name, string? slug = null)
        => new(name, slug, 1, "Apple", null, null);

    private static VariantInput V(string sku, decimal price = 1_000_000m, decimal? compare = null,
        ProductStatus status = ProductStatus.Active)
        => new(sku, "256GB", "Đen", price, compare, status);

    [Fact]
    public async Task Create_makes_parent_model_plus_one_child_variant_with_attributes()
    {
        var db = NewDb();
        var svc = NewService(db);

        var (result, id) = await svc.CreateAsync(P("iPhone 17 Pro Max"), V("SKU-A"));

        result.Should().Be(AdminProductResult.Ok);
        var parent = await db.Products.FindAsync(id);
        parent!.Slug.Should().Be("iphone-17-pro-max");
        parent.ProductParentId.Should().BeNull();
        (await db.Products.CountAsync(p => p.ProductParentId == id)).Should().Be(1);
        // 2 thuộc tính (Dung lượng + Màu) cho biến thể con.
        (await db.ProductAttributes.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task Create_with_duplicate_slug_is_rejected()
    {
        var db = NewDb();
        var svc = NewService(db);
        await svc.CreateAsync(P("iPhone 17"), V("SKU-A"));

        var (result, _) = await svc.CreateAsync(P("iPhone 17"), V("SKU-B"));

        result.Should().Be(AdminProductResult.SlugExists);
        (await db.Products.CountAsync(p => p.ProductParentId == null)).Should().Be(1);
    }

    [Fact]
    public async Task Create_with_duplicate_sku_is_rejected()
    {
        var db = NewDb();
        var svc = NewService(db);
        await svc.CreateAsync(P("iPhone 17"), V("DUP"));

        var (result, _) = await svc.CreateAsync(P("iPhone 16"), V("DUP"));

        result.Should().Be(AdminProductResult.SkuExists);
        (await db.Products.CountAsync(p => p.ProductParentId == null)).Should().Be(1);
    }

    [Fact]
    public async Task AddVariant_appends_child_and_blocks_duplicate_sku()
    {
        var db = NewDb();
        var svc = NewService(db);
        var (_, id) = await svc.CreateAsync(P("iPad Air"), V("AIR-256"));

        var ok = await svc.AddVariantAsync(id, new VariantInput("AIR-512", "512GB", "Xanh", 2_000_000m, null, ProductStatus.Active));
        ok.Should().Be(AdminProductResult.Ok);
        (await db.Products.CountAsync(p => p.ProductParentId == id)).Should().Be(2);

        var dup = await svc.AddVariantAsync(id, new VariantInput("AIR-256", "256GB", "Đỏ", 1_500_000m, null, ProductStatus.Active));
        dup.Should().Be(AdminProductResult.SkuExists);
    }

    [Fact]
    public async Task Toggle_flips_child_status_and_returns_parent_id()
    {
        var db = NewDb();
        var svc = NewService(db);
        var (_, id) = await svc.CreateAsync(P("Watch"), V("W-1", status: ProductStatus.Active));
        var childId = await db.Products.Where(p => p.ProductParentId == id).Select(p => p.Id).FirstAsync();

        var (r1, parentId) = await svc.ToggleVariantAsync(childId);
        r1.Should().Be(AdminProductResult.Ok);
        parentId.Should().Be(id);
        (await db.Products.FindAsync(childId))!.Status.Should().Be(ProductStatus.Discontinued);

        await svc.ToggleVariantAsync(childId);
        (await db.Products.FindAsync(childId))!.Status.Should().Be(ProductStatus.Active);
    }

    [Fact]
    public async Task Update_changes_model_fields_and_child_price()
    {
        var db = NewDb();
        var svc = NewService(db);
        var (_, id) = await svc.CreateAsync(P("Mac mini"), V("MM-1", price: 1_000_000m));
        var childId = await db.Products.Where(p => p.ProductParentId == id).Select(p => p.Id).FirstAsync();

        var r = await svc.UpdateAsync(id, P("Mac mini M4"),
            new[] { new VariantEdit(childId, 1_500_000m, 1_800_000m, ProductStatus.Active) });

        r.Should().Be(AdminProductResult.Ok);
        var parent = await db.Products.FindAsync(id);
        parent!.Name.Should().Be("Mac mini M4");
        parent.Slug.Should().Be("mac-mini-m4");
        (await db.Products.FindAsync(childId))!.BasePrice.Should().Be(1_500_000m);
    }

    [Fact]
    public async Task Update_unknown_product_is_not_found()
    {
        var db = NewDb();
        var r = await NewService(db).UpdateAsync(999, P("X"), Array.Empty<VariantEdit>());
        r.Should().Be(AdminProductResult.NotFound);
    }
}
