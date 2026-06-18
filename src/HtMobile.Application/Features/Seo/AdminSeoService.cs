using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Seo.Dtos;
using HtMobile.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Seo;

/// <summary>
/// Kiểm tra sức khoẻ SEO của catalog (read-only): đếm URL sitemap + liệt kê mục thiếu SEO để admin sửa.
/// Sitemap/robots do storefront (HtMobile.Web) phục vụ; đây là phần app kiểm soát được.
/// </summary>
public class AdminSeoService
{
    private const int SampleLimit = 20;

    private readonly IApplicationDbContext _db;

    public AdminSeoService(IApplicationDbContext db) => _db = db;

    public async Task<SeoOverviewDto> GetOverviewAsync(CancellationToken ct = default)
    {
        var categoryCount = await _db.Categories.CountAsync(ct);
        var modelCount = await _db.Products.CountAsync(p => p.ProductParentId == null, ct);
        // Mỗi danh mục + mỗi model có ≥1 biến thể con = 1 URL trong sitemap (xấp xỉ CatalogService.GetSitemapEntries).
        var modelsWithVariant = await _db.Products.CountAsync(p => p.ProductParentId == null && p.Children.Any(), ct);

        var groups = new List<SeoIssueGroup>
        {
            await BuildProductGroupAsync("no-image", "Sản phẩm thiếu ảnh", "warn",
                p => p.ProductParentId == null && !p.Images.Any(), ct),
            await BuildProductGroupAsync("no-description", "Sản phẩm thiếu mô tả", "warn",
                p => p.ProductParentId == null && (p.Description == null || p.Description == ""), ct),
            await BuildProductGroupAsync("no-active-variant", "Sản phẩm không có biến thể đang bán", "danger",
                p => p.ProductParentId == null && !p.Children.Any(c => c.Status == ProductStatus.Active), ct),
            await BuildCategoryGroupAsync("cat-no-seo", "Danh mục thiếu nội dung SEO", "info",
                c => c.SeoContent == null || c.SeoContent == "", ct),
        };

        return new SeoOverviewDto
        {
            SitemapUrlCount = categoryCount + modelsWithVariant,
            ProductModelCount = modelCount,
            CategoryCount = categoryCount,
            TotalIssues = groups.Sum(g => g.Count),
            Groups = groups,
        };
    }

    private async Task<SeoIssueGroup> BuildProductGroupAsync(
        string key, string title, string severity,
        System.Linq.Expressions.Expression<Func<Domain.Entities.Catalog.Product, bool>> predicate, CancellationToken ct)
    {
        var q = _db.Products.AsNoTracking().Where(predicate);
        var count = await q.CountAsync(ct);
        var items = await q.OrderBy(p => p.Name).Take(SampleLimit)
            .Select(p => new SeoIssueItem("product", p.Id, p.Name)).ToListAsync(ct);
        return new SeoIssueGroup { Key = key, Title = title, Severity = severity, Count = count, Items = items };
    }

    private async Task<SeoIssueGroup> BuildCategoryGroupAsync(
        string key, string title, string severity,
        System.Linq.Expressions.Expression<Func<Domain.Entities.Catalog.Category, bool>> predicate, CancellationToken ct)
    {
        var q = _db.Categories.AsNoTracking().Where(predicate);
        var count = await q.CountAsync(ct);
        var items = await q.OrderBy(c => c.Name).Take(SampleLimit)
            .Select(c => new SeoIssueItem("category", c.Id, c.Name)).ToListAsync(ct);
        return new SeoIssueGroup { Key = key, Title = title, Severity = severity, Count = count, Items = items };
    }
}
