using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Catalog.Dtos;
using HtMobile.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Catalog;

/// <summary>
/// Đọc dữ liệu catalog cho storefront (menu, trang danh mục, PDP). Giá luôn lấy qua <see cref="IPricingService"/>.
/// Mô hình mới (ADR 0003): "sản phẩm" hiển thị = Product cha (ProductParentId == null); "biến thể" = Product con.
/// Thuộc tính (dung lượng/màu…) lấy từ <see cref="ProductAttribute"/>.
/// </summary>
public class CatalogService
{
    private readonly IApplicationDbContext _db;
    private readonly IPricingService _pricing;
    private readonly Bundles.BundleService _bundles;

    public CatalogService(IApplicationDbContext db, IPricingService pricing, Bundles.BundleService bundles)
    {
        _db = db;
        _pricing = pricing;
        _bundles = bundles;
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

    /// <summary>Danh sách URL cho sitemap.xml: danh mục + biến thể mặc định của mỗi sản phẩm (model cha).</summary>
    /// <summary>Trang nội dung tĩnh (CMS) theo slug; null nếu không có.</summary>
    public Task<ContentPageDto?> GetPageAsync(string slug, CancellationToken ct = default)
        => _db.Pages
            .AsNoTracking()
            .Where(p => p.Slug == slug)
            .Select(p => new ContentPageDto(p.Slug, p.Title, p.Body))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<SitemapEntryDto>> GetSitemapEntriesAsync(CancellationToken ct = default)
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .Select(c => new SitemapEntryDto(c.Slug, c.UpdatedAt ?? c.CreatedAt))
            .ToListAsync(ct);

        var products = await _db.Products
            .AsNoTracking()
            .Where(p => p.ProductParentId == null)
            .Select(p => new
            {
                Slug = p.Children.OrderBy(v => v.Id).Select(v => v.Slug).FirstOrDefault(),
                Modified = p.UpdatedAt ?? p.CreatedAt
            })
            .ToListAsync(ct);

        var entries = new List<SitemapEntryDto>(categories);
        entries.AddRange(products
            .Where(p => !string.IsNullOrEmpty(p.Slug))
            .Select(p => new SitemapEntryDto(p.Slug!, p.Modified)));
        return entries;
    }

    /// <summary>Trang kết quả tìm kiếm theo từ khóa (khớp tên sản phẩm model).</summary>
    public async Task<SearchPageDto> SearchAsync(string query, int take = 24, CancellationToken ct = default)
    {
        query = query?.Trim() ?? string.Empty;
        if (query.Length < 2)
            return new SearchPageDto { Query = query };

        // Provider-agnostic (Application không ref Npgsql): EF dịch sang lower(Name) LIKE '%q%'.
        var q = query.ToLower();
        var products = await BuildCardsAsync(p => p.Name.ToLower().Contains(q), take, ct);
        return new SearchPageDto { Query = query, Products = products };
    }

    /// <summary>PDP theo slug của 1 biến thể (Product con) — slug riêng từng biến thể.</summary>
    public async Task<ProductDetailDto?> GetByVariantSlugAsync(string variantSlug, CancellationToken ct = default)
    {
        var hit = await _db.Products
            .AsNoTracking()
            .Where(p => p.Slug == variantSlug)
            .Select(p => new { p.Id, p.ProductParentId })
            .FirstOrDefaultAsync(ct);
        if (hit is null) return null;

        // Nếu slug trỏ model cha → chọn con đầu; nếu trỏ con → model = cha của nó.
        if (hit.ProductParentId is null)
            return await GetByProductSlugInternalAsync(hit.Id, ct);

        return await BuildDetailAsync(hit.ProductParentId.Value, hit.Id, ct);
    }

    /// <summary>PDP theo slug sản phẩm (model cha) — chọn biến thể con đầu tiên làm mặc định.</summary>
    public async Task<ProductDetailDto?> GetByProductSlugAsync(string productSlug, CancellationToken ct = default)
    {
        var model = await _db.Products
            .AsNoTracking()
            .Where(p => p.Slug == productSlug)
            .Select(p => new { p.Id, p.ProductParentId })
            .FirstOrDefaultAsync(ct);
        if (model is null) return null;

        // Slug là con → suy ra cha; slug là cha → dùng chính nó.
        var modelId = model.ProductParentId ?? model.Id;
        return await GetByProductSlugInternalAsync(modelId, ct);
    }

    private async Task<ProductDetailDto?> GetByProductSlugInternalAsync(long modelId, CancellationToken ct)
    {
        var firstChildId = await _db.Products
            .AsNoTracking()
            .Where(p => p.ProductParentId == modelId)
            .OrderBy(p => p.Id)
            .Select(p => (long?)p.Id)
            .FirstOrDefaultAsync(ct);
        if (firstChildId is null) return null;

        return await BuildDetailAsync(modelId, firstChildId.Value, ct);
    }

    private async Task<ProductDetailDto?> BuildDetailAsync(long modelId, long selectedVariantId, CancellationToken ct)
    {
        var now = DateTime.Now;

        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == modelId, ct);
        if (product is null) return null;

        var category = await _db.Categories
            .AsNoTracking()
            .Where(c => c.Id == product.CategoryId)
            .Select(c => new { c.Name, c.Slug })
            .FirstOrDefaultAsync(ct);

        // Biến thể = Product con của model; nhãn ghép từ ProductAttribute (theo SortOrder của Attribute).
        var variantRows = await _db.Products
            .AsNoTracking()
            .Where(p => p.ProductParentId == modelId)
            .OrderBy(p => p.Id)
            .Select(p => new
            {
                p.Id,
                p.Slug,
                p.Sku,
                p.Status,
                Attrs = p.Attributes.OrderBy(a => a.Attribute.SortOrder).ThenBy(a => a.AttributeId)
                    .Select(a => new { a.Attribute.Name, a.Attribute.SortOrder, a.Value }).ToList()
            })
            .ToListAsync(ct);

        var variants = variantRows
            .Select(v => new VariantOptionDto(v.Id, v.Slug, JoinAttrs(v.Attrs.Select(a => a.Value)), v.Sku ?? string.Empty, v.Id == selectedVariantId))
            .ToList();

        // Bộ chọn tách theo trục thuộc tính (Dung lượng / Màu…).
        var variantInfos = variantRows
            .Select(v => new VariantInfo(v.Id, v.Slug, v.Status == Domain.Enums.ProductStatus.Active,
                v.Attrs.Select(a => new VariantAttr(a.Name, a.SortOrder, a.Value)).ToList()))
            .ToList();
        var variantAxes = VariantAxisBuilder.Build(variantInfos, selectedVariantId);

        // Giá từng biến thể (nhúng client để đổi giá tức thì khi chọn). Giá hiệu lực cache ở Redis nên rẻ.
        var variantPrices = new List<VariantPriceDto>(variantRows.Count);
        foreach (var v in variantRows)
        {
            var vp = await _pricing.GetEffectivePriceAsync(v.Id, ct);
            variantPrices.Add(new VariantPriceDto(
                v.Id, v.Slug, v.Sku ?? string.Empty, v.Status == Domain.Enums.ProductStatus.Active,
                vp.FinalPrice, vp.CompareAtPrice, vp.DiscountPercent,
                v.Attrs.ToDictionary(a => a.Name, a => a.Value)));
        }

        var selected = variantRows.FirstOrDefault(v => v.Id == selectedVariantId);
        var canonicalSlug = variantRows.FirstOrDefault()?.Slug ?? product.Slug;

        var images = await _db.ProductImages
            .AsNoTracking()
            .Where(i => i.ProductId == modelId)
            .OrderBy(i => i.SortOrder)
            .Select(i => i.Url)
            .ToListAsync(ct);

        var videos = await _db.ProductVideos
            .AsNoTracking()
            .Where(v => v.ProductId == modelId)
            .Select(v => v.YoutubeUrl)
            .ToListAsync(ct);

        var offers = await _db.Promotions
            .AsNoTracking()
            .Where(p => p.StartsAt <= now && p.EndsAt >= now)
            .OrderBy(p => p.EndsAt)
            .Select(p => new OfferDto(p.Name, p.EndsAt))
            .ToListAsync(ct);

        var paymentOffers = await _db.PaymentPromotions
            .AsNoTracking()
            .Where(p => p.StartsAt <= now && p.EndsAt >= now)
            .OrderBy(p => p.EndsAt)
            .Select(p => new PaymentOfferDto(p.Bank, p.Title, p.Description, p.EndsAt))
            .ToListAsync(ct);

        // Đánh giá gộp theo model: review của model hoặc bất kỳ biến thể con nào thuộc model này.
        var reviewRows = await _db.Reviews
            .AsNoTracking()
            .Where(r => r.Product.ProductParentId == modelId || r.ProductId == modelId)
            .OrderByDescending(r => r.Id)
            .Select(r => new
            {
                r.Rating,
                r.Content,
                r.CreatedAt,
                Author = r.CustomerId == null
                    ? null
                    : _db.Customers.Where(c => c.Id == r.CustomerId).Select(c => c.FullName ?? c.Email).FirstOrDefault()
            })
            .ToListAsync(ct);

        var reviews = reviewRows
            .Take(10)
            .Select(r => new ReviewDto(string.IsNullOrWhiteSpace(r.Author) ? "Khách hàng" : r.Author!, r.Rating, r.Content, r.CreatedAt))
            .ToList();
        var ratingCounts = reviewRows.GroupBy(r => r.Rating).ToDictionary(g => g.Key, g => g.Count());
        var reviewCount = reviewRows.Count;
        double? avgRating = reviewCount == 0 ? null : reviewRows.Average(r => r.Rating);

        var price = await _pricing.GetEffectivePriceAsync(selectedVariantId, ct);
        var bundle = await _bundles.GetForProductAsync(modelId, ct);

        // Sản phẩm liên quan: cùng danh mục, trừ chính model này (tối đa 4).
        var related = await BuildCardsAsync(p => p.CategoryId == product.CategoryId && p.Id != modelId, 4, ct);

        return new ProductDetailDto
        {
            ProductId = product.Id,
            Name = product.Name,
            ProductSlug = product.Slug,
            Tagline = product.Tagline,
            Brand = product.Brand,
            Description = product.Description,
            Specs = ProductSpecs.Parse(product.SpecsJson),
            CategoryName = category?.Name ?? string.Empty,
            CategorySlug = category?.Slug ?? string.Empty,
            SelectedVariantId = selectedVariantId,
            CanonicalSlug = canonicalSlug,
            SelectedVariantLabel = selected is null ? null : JoinAttrsOrNull(selected.Attrs.Select(a => a.Value).ToList()),
            SelectedSku = selected?.Sku ?? string.Empty,
            InStock = selected?.Status == Domain.Enums.ProductStatus.Active,
            Price = price,
            Variants = variants,
            VariantAxes = variantAxes,
            VariantPrices = variantPrices,
            Images = images,
            YoutubeUrls = videos,
            Offers = offers,
            PaymentOffers = paymentOffers,
            Bundle = bundle,
            ReviewCount = reviewCount,
            AverageRating = avgRating,
            Related = related,
            Reviews = reviews,
            RatingCounts = ratingCounts
        };
    }

    private static string JoinAttrs(IEnumerable<string> values) => string.Join(" · ", values);
    private static string? JoinAttrsOrNull(IReadOnlyCollection<string> values) => values.Count == 0 ? null : string.Join(" · ", values);

    /// <summary>Dựng thẻ sản phẩm: mỗi model (cha) + biến thể con đại diện (con đầu) + giá hiệu lực.</summary>
    private async Task<IReadOnlyList<ProductCardDto>> BuildCardsAsync(
        System.Linq.Expressions.Expression<Func<Product, bool>> filter, int take, CancellationToken ct)
    {
        var now = DateTime.Now;
        var products = await _db.Products
            .AsNoTracking()
            .Where(p => p.ProductParentId == null)
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
                Variant = p.Children.OrderBy(v => v.Id).Select(v => new { v.Id, v.Slug }).FirstOrDefault(),
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
                IsNew = CatalogBadge.IsNew(p.CreatedAt, now),
                Series = CatalogBadge.Series(p.Name)
            });
        }
        return cards;
    }
}
