using HtMobile.Application.Common;
using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Catalog.Dtos;
using HtMobile.Domain.Entities.Catalog;
using HtMobile.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Catalog;

/// <summary>
/// CRUD sản phẩm + biến thể cho Admin (P5-02). Slug auto-gen từ Name (<see cref="Slugify"/>), unique.
/// Giá set thẳng BasePrice/CompareAtPrice (đụng tiền → review Cổng 6). Truy cập DB qua IApplicationDbContext.
/// </summary>
public class AdminProductService
{
    private readonly IApplicationDbContext _db;

    public AdminProductService(IApplicationDbContext db) => _db = db;

    public async Task<AdminProductListDto> GetListAsync(long? categoryId, CancellationToken ct = default)
    {
        var query = _db.Products.AsNoTracking();
        if (categoryId is not null) query = query.Where(p => p.CategoryId == categoryId);

        var rows = await query
            .OrderByDescending(p => p.Id)
            .Select(p => new AdminProductRow
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                CategoryName = p.Category.Name,
                Sku = p.Variants.OrderBy(v => v.Id).Select(v => v.Sku).FirstOrDefault(),
                VariantCount = p.Variants.Count,
                PriceFrom = p.Variants.Min(v => (decimal?)v.BasePrice) ?? 0m,
                PriceTo = p.Variants.Max(v => (decimal?)v.BasePrice) ?? 0m,
                IsActive = p.Variants.Any(v => v.Status == VariantStatus.Active)
            })
            .ToListAsync(ct);

        return new AdminProductListDto
        {
            Products = rows,
            Categories = await GetCategoryOptionsAsync(ct),
            CategoryId = categoryId
        };
    }

    public async Task<IReadOnlyList<AdminCategoryOption>> GetCategoryOptionsAsync(CancellationToken ct = default)
        => await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new AdminCategoryOption(c.Id, c.Name))
            .ToListAsync(ct);

    public async Task<AdminProductEditDto?> GetEditAsync(long id, CancellationToken ct = default)
    {
        var p = await _db.Products
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AdminProductEditDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                CategoryId = x.CategoryId,
                Brand = x.Brand,
                Tagline = x.Tagline,
                Description = x.Description,
                Variants = x.Variants.OrderBy(v => v.Id).Select(v => new AdminVariantRow
                {
                    Id = v.Id,
                    Sku = v.Sku,
                    Storage = v.Storage,
                    Color = v.Color,
                    BasePrice = v.BasePrice,
                    CompareAtPrice = v.CompareAtPrice,
                    Status = v.Status
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);
        if (p is null) return null;

        p.Categories = await GetCategoryOptionsAsync(ct);
        return p;
    }

    public async Task<(AdminProductResult Result, long Id)> CreateAsync(ProductInput input, VariantInput firstVariant, CancellationToken ct = default)
    {
        var slug = ResolveSlug(input.Slug, input.Name);
        if (await _db.Products.AnyAsync(p => p.Slug == slug, ct))
            return (AdminProductResult.SlugExists, 0);

        var variant = NewVariant(firstVariant, slug);
        if (await VariantConflictAsync(variant.Sku, variant.Slug, ct))
            return (AdminProductResult.SkuExists, 0);

        var product = new Product
        {
            CategoryId = input.CategoryId,
            Name = input.Name.Trim(),
            Slug = slug,
            Brand = Trim(input.Brand),
            Tagline = Trim(input.Tagline),
            Description = Trim(input.Description)
        };
        product.Variants.Add(variant);
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        return (AdminProductResult.Ok, product.Id);
    }

    public async Task<AdminProductResult> UpdateAsync(long id, ProductInput input, IReadOnlyList<VariantEdit> variants, CancellationToken ct = default)
    {
        var product = await _db.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null) return AdminProductResult.NotFound;

        var slug = ResolveSlug(input.Slug, input.Name);
        if (slug != product.Slug && await _db.Products.AnyAsync(p => p.Slug == slug && p.Id != id, ct))
            return AdminProductResult.SlugExists;

        product.Name = input.Name.Trim();
        product.Slug = slug;
        product.CategoryId = input.CategoryId;
        product.Brand = Trim(input.Brand);
        product.Tagline = Trim(input.Tagline);
        product.Description = Trim(input.Description);

        foreach (var edit in variants)
        {
            var v = product.Variants.FirstOrDefault(x => x.Id == edit.Id);
            if (v is null) continue;
            v.BasePrice = edit.BasePrice;
            v.CompareAtPrice = edit.CompareAtPrice;
            v.Status = edit.Status;
        }

        await _db.SaveChangesAsync(ct);
        return AdminProductResult.Ok;
    }

    /// <summary>Thêm 1 biến thể vào SP có sẵn (dùng cho AJAX). Slug biến thể sinh từ slug SP + Sku/Storage/Color.</summary>
    public async Task<AdminProductResult> AddVariantAsync(long productId, VariantInput input, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
        if (product is null) return AdminProductResult.NotFound;

        var variant = NewVariant(input, product.Slug);
        if (await VariantConflictAsync(variant.Sku, variant.Slug, ct))
            return AdminProductResult.SkuExists;

        variant.ProductId = productId;
        _db.ProductVariants.Add(variant);
        await _db.SaveChangesAsync(ct);
        return AdminProductResult.Ok;
    }

    /// <summary>Ẩn/hiện biến thể: Active ⇄ Discontinued (dùng cho AJAX toggle). Trả kèm ProductId (lấy từ DB, không tin client).</summary>
    public async Task<(AdminProductResult Result, long ProductId)> ToggleVariantAsync(long variantId, CancellationToken ct = default)
    {
        var v = await _db.ProductVariants.FirstOrDefaultAsync(x => x.Id == variantId, ct);
        if (v is null) return (AdminProductResult.NotFound, 0);

        v.Status = v.Status == VariantStatus.Active ? VariantStatus.Discontinued : VariantStatus.Active;
        await _db.SaveChangesAsync(ct);
        return (AdminProductResult.Ok, v.ProductId);
    }

    /// <summary>SKU hoặc slug biến thể đã tồn tại? (unique index ở DB — chặn sớm để không vỡ 500.)</summary>
    private async Task<bool> VariantConflictAsync(string sku, string slug, CancellationToken ct)
        => await _db.ProductVariants.AnyAsync(v => v.Sku == sku || v.Slug == slug, ct);

    private static ProductVariant NewVariant(VariantInput input, string productSlug)
    {
        var suffix = Slugify.ToSlug(string.Join(" ", new[] { input.Storage, input.Color, input.Sku }
            .Where(s => !string.IsNullOrWhiteSpace(s))));
        var slug = string.IsNullOrEmpty(suffix) ? productSlug : $"{productSlug}-{suffix}";
        return new ProductVariant
        {
            Sku = input.Sku.Trim(),
            Storage = Trim(input.Storage),
            Color = Trim(input.Color),
            Slug = slug,
            BasePrice = input.BasePrice,
            CompareAtPrice = input.CompareAtPrice,
            Status = input.Status
        };
    }

    private static string ResolveSlug(string? slug, string name)
        => string.IsNullOrWhiteSpace(slug) ? Slugify.ToSlug(name) : Slugify.ToSlug(slug);

    private static string? Trim(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
