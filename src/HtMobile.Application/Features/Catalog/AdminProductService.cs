using HtMobile.Application.Common;
using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Catalog.Dtos;
using HtMobile.Domain.Entities.Catalog;
using HtMobile.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using AttributeEntity = HtMobile.Domain.Entities.Catalog.Attribute;

namespace HtMobile.Application.Features.Catalog;

/// <summary>
/// CRUD sản phẩm + biến thể cho Admin (P5-02), theo mô hình mới (ADR 0003): model = Product cha,
/// biến thể = Product con (ProductParentId). Admin quản lý 2 thuộc tính phổ biến "Dung lượng" + "Màu"
/// (lưu ở ProductAttribute); thuộc tính khác là feature sau. Giá set thẳng trên Product con.
/// </summary>
public class AdminProductService
{
    public const string StorageAttr = "Dung lượng";
    public const string ColorAttr = "Màu";

    private readonly IApplicationDbContext _db;
    private readonly IDateTime _clock;
    private readonly IPricingService _pricing;

    public AdminProductService(IApplicationDbContext db, IDateTime clock, IPricingService pricing)
    {
        _db = db;
        _clock = clock;
        _pricing = pricing;
    }

    public async Task<AdminProductListDto> GetListAsync(long? categoryId, CancellationToken ct = default)
    {
        var query = _db.Products.AsNoTracking().Where(p => p.ProductParentId == null);
        if (categoryId is not null) query = query.Where(p => p.CategoryId == categoryId);

        var rows = await query
            .OrderByDescending(p => p.Id)
            .Select(p => new AdminProductRow
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                CategoryName = p.Category.Name,
                Sku = p.Children.OrderBy(v => v.Id).Select(v => v.Sku).FirstOrDefault(),
                VariantCount = p.Children.Count,
                PriceFrom = p.Children.Min(v => (decimal?)v.BasePrice) ?? 0m,
                PriceTo = p.Children.Max(v => (decimal?)v.BasePrice) ?? 0m,
                IsActive = p.Children.Any(v => v.Status == ProductStatus.Active)
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
            .Where(x => x.Id == id && x.ProductParentId == null)
            .Select(x => new AdminProductEditDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                CategoryId = x.CategoryId,
                Brand = x.Brand,
                Tagline = x.Tagline,
                Description = x.Description,
                Specs = x.SpecsJson,
                Variants = x.Children.OrderBy(v => v.Id).Select(v => new AdminVariantRow
                {
                    Id = v.Id,
                    Sku = v.Sku ?? string.Empty,
                    Storage = v.Attributes.Where(a => a.Attribute.Name == StorageAttr).Select(a => a.Value).FirstOrDefault(),
                    Color = v.Attributes.Where(a => a.Attribute.Name == ColorAttr).Select(a => a.Value).FirstOrDefault(),
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
        if (!ProductSpecs.IsValid(input.Specs)) return (AdminProductResult.InvalidSpecs, 0);

        var slug = ResolveSlug(input.Slug, input.Name);
        if (await _db.Products.AnyAsync(p => p.Slug == slug, ct))
            return (AdminProductResult.SlugExists, 0);

        var childSlug = BuildVariantSlug(firstVariant, slug);
        if (await VariantConflictAsync(firstVariant.Sku, childSlug, ct))
            return (AdminProductResult.SkuExists, 0);

        // Model cha (gom, không bán trực tiếp): giá 0, không SKU.
        var parent = new Product
        {
            CategoryId = input.CategoryId,
            Name = input.Name.Trim(),
            Slug = slug,
            Brand = Trim(input.Brand),
            Tagline = Trim(input.Tagline),
            Description = Trim(input.Description),
            SpecsJson = Trim(input.Specs),
            Status = ProductStatus.Active
        };
        var child = await NewVariantAsync(firstVariant, parent, childSlug, ct);
        parent.Children.Add(child);
        _db.Products.Add(parent);
        await _db.SaveChangesAsync(ct);
        return (AdminProductResult.Ok, parent.Id);
    }

    public async Task<AdminProductResult> UpdateAsync(long id, ProductInput input, IReadOnlyList<VariantEdit> variants, CancellationToken ct = default)
    {
        if (!ProductSpecs.IsValid(input.Specs)) return AdminProductResult.InvalidSpecs;

        var product = await _db.Products
            .Include(p => p.Children)
            .FirstOrDefaultAsync(p => p.Id == id && p.ProductParentId == null, ct);
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
        product.SpecsJson = Trim(input.Specs);

        var priceChanged = new List<long>();
        foreach (var edit in variants)
        {
            var v = product.Children.FirstOrDefault(x => x.Id == edit.Id);
            if (v is null) continue;
            if (v.BasePrice != edit.BasePrice || v.CompareAtPrice != edit.CompareAtPrice) priceChanged.Add(v.Id);
            v.BasePrice = edit.BasePrice;
            v.CompareAtPrice = edit.CompareAtPrice;
            v.Status = edit.Status;
        }

        await _db.SaveChangesAsync(ct);
        // Xoá cache giá cho biến thể đổi giá (tránh PDP/giỏ phục vụ giá cũ trong TTL).
        foreach (var id2 in priceChanged) await _pricing.InvalidateAsync(id2, ct);
        return AdminProductResult.Ok;
    }

    /// <summary>Thêm 1 biến thể (Product con) vào model có sẵn (dùng cho AJAX).</summary>
    public async Task<AdminProductResult> AddVariantAsync(long productId, VariantInput input, CancellationToken ct = default)
    {
        var parent = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.ProductParentId == null, ct);
        if (parent is null) return AdminProductResult.NotFound;

        var childSlug = BuildVariantSlug(input, parent.Slug);
        if (await VariantConflictAsync(input.Sku, childSlug, ct))
            return AdminProductResult.SkuExists;

        var child = await NewVariantAsync(input, parent, childSlug, ct);
        child.ProductParentId = parent.Id;   // gắn vào model cha
        _db.Products.Add(child);
        await _db.SaveChangesAsync(ct);
        return AdminProductResult.Ok;
    }

    /// <summary>Ẩn/hiện biến thể: Active ⇄ Discontinued. Trả kèm Id model cha (re-render màn sửa).</summary>
    public async Task<(AdminProductResult Result, long ProductId)> ToggleVariantAsync(long variantId, CancellationToken ct = default)
    {
        var v = await _db.Products.FirstOrDefaultAsync(x => x.Id == variantId && x.ProductParentId != null, ct);
        if (v is null) return (AdminProductResult.NotFound, 0);

        v.Status = v.Status == ProductStatus.Active ? ProductStatus.Discontinued : ProductStatus.Active;
        await _db.SaveChangesAsync(ct);
        return (AdminProductResult.Ok, v.ProductParentId!.Value);
    }

    /// <summary>Tạo Product con (biến thể) + gắn thuộc tính Dung lượng/Màu. Chưa add vào context.</summary>
    private async Task<Product> NewVariantAsync(VariantInput input, Product parent, string childSlug, CancellationToken ct)
    {
        var child = new Product
        {
            CategoryId = parent.CategoryId,
            Name = parent.Name,
            Slug = childSlug,
            Sku = input.Sku.Trim(),
            BasePrice = input.BasePrice,
            CompareAtPrice = input.CompareAtPrice,
            Status = input.Status
        };

        await AddAttrAsync(child, StorageAttr, sortOrder: 1, input.Storage, ct);
        await AddAttrAsync(child, ColorAttr, sortOrder: 2, input.Color, ct);
        return child;
    }

    private async Task AddAttrAsync(Product child, string attrName, int sortOrder, string? value, CancellationToken ct)
    {
        value = Trim(value);
        if (value is null) return;
        var attrId = await GetOrCreateAttributeAsync(attrName, sortOrder, ct);
        child.Attributes.Add(new ProductAttribute { AttributeId = attrId, Value = value, CreatedDate = _clock.Now });
    }

    private async Task<long> GetOrCreateAttributeAsync(string name, int sortOrder, CancellationToken ct)
    {
        var existing = await _db.Attributes.FirstOrDefaultAsync(a => a.Name == name, ct);
        if (existing is not null) return existing.Id;

        var attr = new AttributeEntity { Name = name, SortOrder = sortOrder };
        _db.Attributes.Add(attr);
        await _db.SaveChangesAsync(ct);   // cần Id trước khi tham chiếu
        return attr.Id;
    }

    private static string BuildVariantSlug(VariantInput input, string productSlug)
    {
        var suffix = Slugify.ToSlug(string.Join(" ", new[] { input.Storage, input.Color, input.Sku }
            .Where(s => !string.IsNullOrWhiteSpace(s))));
        return string.IsNullOrEmpty(suffix) ? productSlug : $"{productSlug}-{suffix}";
    }

    /// <summary>SKU hoặc slug biến thể đã tồn tại? (unique trên Product) — chặn sớm tránh 500.</summary>
    private async Task<bool> VariantConflictAsync(string sku, string slug, CancellationToken ct)
    {
        var s = sku.Trim();
        return await _db.Products.AnyAsync(p => p.Sku == s || p.Slug == slug, ct);
    }

    private static string ResolveSlug(string? slug, string name)
        => string.IsNullOrWhiteSpace(slug) ? Slugify.ToSlug(name) : Slugify.ToSlug(slug);

    private static string? Trim(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
