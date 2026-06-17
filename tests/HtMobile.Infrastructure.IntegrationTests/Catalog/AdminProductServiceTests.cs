using FluentAssertions;
using HtMobile.Application.Features.Catalog;
using HtMobile.Application.Features.Catalog.Dtos;
using HtMobile.Domain.Entities.Catalog;
using HtMobile.Domain.Enums;
using HtMobile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Infrastructure.IntegrationTests.Catalog;

/// <summary>AdminProductService trên EF InMemory: slug auto-gen + unique, SKU unique, toggle, thêm biến thể.</summary>
public class AdminProductServiceTests
{
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

    private static ProductInput P(string name, string? slug = null)
        => new(name, slug, 1, "Apple", null, null);

    private static VariantInput V(string sku, decimal price = 1_000_000m, decimal? compare = null,
        VariantStatus status = VariantStatus.Active)
        => new(sku, "256GB", "Đen", price, compare, status);

    [Fact]
    public async Task Create_auto_generates_unique_slug_from_name()
    {
        var db = NewDb();
        var svc = new AdminProductService(db);

        var (result, id) = await svc.CreateAsync(P("iPhone 17 Pro Max"), V("SKU-A"));

        result.Should().Be(AdminProductResult.Ok);
        var p = await db.Products.FindAsync(id);
        p!.Slug.Should().Be("iphone-17-pro-max");
        (await db.ProductVariants.CountAsync(v => v.ProductId == id)).Should().Be(1);
    }

    [Fact]
    public async Task Create_with_duplicate_slug_is_rejected()
    {
        var db = NewDb();
        var svc = new AdminProductService(db);
        await svc.CreateAsync(P("iPhone 17"), V("SKU-A"));

        var (result, _) = await svc.CreateAsync(P("iPhone 17"), V("SKU-B"));

        result.Should().Be(AdminProductResult.SlugExists);
        (await db.Products.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Create_with_duplicate_sku_is_rejected()
    {
        var db = NewDb();
        var svc = new AdminProductService(db);
        await svc.CreateAsync(P("iPhone 17"), V("DUP"));

        var (result, _) = await svc.CreateAsync(P("iPhone 16"), V("DUP"));

        result.Should().Be(AdminProductResult.SkuExists);
        (await db.Products.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task AddVariant_appends_and_blocks_duplicate_sku()
    {
        var db = NewDb();
        var svc = new AdminProductService(db);
        var (_, id) = await svc.CreateAsync(P("iPad Air"), V("AIR-256"));

        var ok = await svc.AddVariantAsync(id, new VariantInput("AIR-512", "512GB", "Xanh", 2_000_000m, null, VariantStatus.Active));
        ok.Should().Be(AdminProductResult.Ok);
        (await db.ProductVariants.CountAsync(v => v.ProductId == id)).Should().Be(2);

        var dup = await svc.AddVariantAsync(id, new VariantInput("AIR-256", "256GB", "Đỏ", 1_500_000m, null, VariantStatus.Active));
        dup.Should().Be(AdminProductResult.SkuExists);
    }

    [Fact]
    public async Task Toggle_flips_status_and_returns_product_id()
    {
        var db = NewDb();
        var svc = new AdminProductService(db);
        var (_, id) = await svc.CreateAsync(P("Watch"), V("W-1", status: VariantStatus.Active));
        var variantId = await db.ProductVariants.Where(v => v.ProductId == id).Select(v => v.Id).FirstAsync();

        var (r1, pid1) = await svc.ToggleVariantAsync(variantId);
        r1.Should().Be(AdminProductResult.Ok);
        pid1.Should().Be(id);
        (await db.ProductVariants.FindAsync(variantId))!.Status.Should().Be(VariantStatus.Discontinued);

        await svc.ToggleVariantAsync(variantId);
        (await db.ProductVariants.FindAsync(variantId))!.Status.Should().Be(VariantStatus.Active);
    }

    [Fact]
    public async Task Update_changes_fields_and_variant_price()
    {
        var db = NewDb();
        var svc = new AdminProductService(db);
        var (_, id) = await svc.CreateAsync(P("Mac mini"), V("MM-1", price: 1_000_000m));
        var variantId = await db.ProductVariants.Where(v => v.ProductId == id).Select(v => v.Id).FirstAsync();

        var r = await svc.UpdateAsync(id, P("Mac mini M4"),
            new[] { new VariantEdit(variantId, 1_500_000m, 1_800_000m, VariantStatus.Active) });

        r.Should().Be(AdminProductResult.Ok);
        var p = await db.Products.FindAsync(id);
        p!.Name.Should().Be("Mac mini M4");
        p.Slug.Should().Be("mac-mini-m4");
        (await db.ProductVariants.FindAsync(variantId))!.BasePrice.Should().Be(1_500_000m);
    }

    [Fact]
    public async Task Update_unknown_product_is_not_found()
    {
        var db = NewDb();
        var r = await new AdminProductService(db).UpdateAsync(999, P("X"), Array.Empty<VariantEdit>());
        r.Should().Be(AdminProductResult.NotFound);
    }
}
