using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Catalog.Dtos;
using HtMobile.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Catalog;

/// <summary>
/// Đọc dữ liệu catalog cho storefront (menu, trang danh mục, PDP). Giá luôn lấy qua <see cref="IPricingService"/>.
/// </summary>
public class CatalogService
{
    private readonly IApplicationDbContext _db;
    private readonly IPricingService _pricing;

    public CatalogService(IApplicationDbContext db, IPricingService pricing)
    {
        _db = db;
        _pricing = pricing;
    }

    /// <summary>Danh mục gốc cho menu điều hướng.</summary>
    public async Task<IReadOnlyList<CategoryDto>> GetMenuAsync(CancellationToken ct = default)
    {
        return await _db.Categories
            .AsNoTracking()
            .Where(c => c.ParentId == null)
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Slug, c.SortOrder))
            .ToListAsync(ct);
    }

    /// <summary>Trang chủ: banner slider + tile/nav danh mục + lưới sản phẩm nổi bật.</summary>
    public async Task<HomePageDto> GetHomePageAsync(int featuredCount = 8, CancellationToken ct = default)
    {
        var now = DateTime.Now;

        var activeBanners = await _db.Banners
            .AsNoTracking()
            .Where(b => b.IsActive)
            .OrderBy(b => b.SortOrder)
            .Take(10)
            .ToListAsync(ct);

        var banners = activeBanners
            .Where(b => b.IsActiveAt(now))
            .Select(b => new BannerDto(b.Eyebrow, b.Title, b.Subtitle, b.ImageUrl, b.LinkUrl, b.CtaText))
            .ToList();

        var categories = await GetMenuAsync(ct);
        var featured = await BuildCardsAsync(_ => true, featuredCount, ct);

        return new HomePageDto { Banners = banners, Categories = categories, Featured = featured };
    }

    /// <summary>Trang danh mục theo slug + lưới sản phẩm.</summary>
    public async Task<CategoryPageDto?> GetCategoryPageAsync(string slug, int take = 24, CancellationToken ct = default)
    {
        var category = await _db.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);
        if (category is null) return null;

        var products = await BuildCardsAsync(p => p.CategoryId == category.Id, take, ct);

        return new CategoryPageDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            SeoContent = category.SeoContent,
            Products = products
        };
    }

    /// <summary>PDP theo slug của 1 biến thể (URL riêng cho mỗi variant).</summary>
    public async Task<ProductDetailDto?> GetByVariantSlugAsync(string variantSlug, CancellationToken ct = default)
    {
        var variant = await _db.ProductVariants
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Slug == variantSlug, ct);
        if (variant is null) return null;

        return await BuildDetailAsync(variant.ProductId, variant.Id, ct);
    }

    /// <summary>PDP theo slug sản phẩm — chọn biến thể đầu tiên làm mặc định.</summary>
    public async Task<ProductDetailDto?> GetByProductSlugAsync(string productSlug, CancellationToken ct = default)
    {
        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == productSlug, ct);
        if (product is null) return null;

        var firstVariant = await _db.ProductVariants
            .AsNoTracking()
            .Where(v => v.ProductId == product.Id)
            .OrderBy(v => v.Id)
            .FirstOrDefaultAsync(ct);
        if (firstVariant is null) return null;

        return await BuildDetailAsync(product.Id, firstVariant.Id, ct);
    }

    private async Task<ProductDetailDto?> BuildDetailAsync(long productId, long selectedVariantId, CancellationToken ct)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId, ct);
        if (product is null) return null;

        var variants = await _db.ProductVariants
            .AsNoTracking()
            .Where(v => v.ProductId == productId)
            .OrderBy(v => v.Id)
            .Select(v => new VariantOptionDto(v.Id, v.Slug, v.Storage, v.Color, v.Sku, v.Id == selectedVariantId))
            .ToListAsync(ct);

        var images = await _db.ProductImages
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.SortOrder)
            .Select(i => i.Url)
            .ToListAsync(ct);

        var videos = await _db.ProductVideos
            .AsNoTracking()
            .Where(v => v.ProductId == productId)
            .Select(v => v.YoutubeUrl)
            .ToListAsync(ct);

        var price = await _pricing.GetEffectivePriceAsync(selectedVariantId, ct);

        return new ProductDetailDto
        {
            ProductId = product.Id,
            Name = product.Name,
            ProductSlug = product.Slug,
            Description = product.Description,
            SpecsJson = product.SpecsJson,
            SelectedVariantId = selectedVariantId,
            Price = price,
            Variants = variants,
            Images = images,
            YoutubeUrls = videos
        };
    }

    /// <summary>Dựng thẻ sản phẩm: lấy biến thể đại diện của mỗi sản phẩm + giá hiệu lực.</summary>
    private async Task<IReadOnlyList<ProductCardDto>> BuildCardsAsync(
        System.Linq.Expressions.Expression<Func<Product, bool>> filter, int take, CancellationToken ct)
    {
        var now = DateTime.Now;
        var products = await _db.Products
            .AsNoTracking()
            .Where(filter)
            .OrderByDescending(p => p.Id)
            .Take(take)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Tagline,
                p.Slug,
                p.CreatedAt,
                Variant = p.Variants.OrderBy(v => v.Id).Select(v => new { v.Id, v.Slug }).FirstOrDefault(),
                Thumbnail = p.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault()
            })
            .ToListAsync(ct);

        var cards = new List<ProductCardDto>();
        foreach (var p in products)
        {
            if (p.Variant is null) continue;
            var price = await _pricing.GetEffectivePriceAsync(p.Variant.Id, ct);
            cards.Add(new ProductCardDto
            {
                ProductId = p.Id,
                Name = p.Name,
                Tagline = p.Tagline,
                ProductSlug = p.Slug,
                VariantId = p.Variant.Id,
                VariantSlug = p.Variant.Slug,
                ThumbnailUrl = p.Thumbnail,
                FinalPrice = price.FinalPrice,
                CompareAtPrice = price.CompareAtPrice,
                DiscountPercent = price.DiscountPercent,
                IsNew = CatalogBadge.IsNew(p.CreatedAt, now)
            });
        }
        return cards;
    }
}
