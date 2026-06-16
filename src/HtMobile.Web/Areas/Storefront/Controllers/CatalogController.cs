using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Catalog;
using HtMobile.Web.Infrastructure;
using HtMobile.Web.Infrastructure.Routing;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Storefront.Controllers;

[Area("Storefront")]
public class CatalogController : Controller
{
    private readonly CatalogService _catalog;
    private readonly ISlugResolver _resolver;
    private readonly ISearchService _search;
    private readonly ICurrentRegion _region;

    public CatalogController(CatalogService catalog, ISlugResolver resolver, ISearchService search, ICurrentRegion region)
    {
        _catalog = catalog;
        _resolver = resolver;
        _search = search;
        _region = region;
    }

    /// <summary>Đổi vùng giá (lưu cookie) rồi quay lại trang trước.</summary>
    [HttpGet("/set-region")]
    public IActionResult SetRegion(long regionId, string? returnUrl)
    {
        Response.Cookies.Append(CurrentRegion.CookieName, regionId.ToString(), new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true
        });
        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }

    /// <summary>Gợi ý tìm kiếm cho htmx (trả partial HTML).</summary>
    [HttpGet("/search/suggest")]
    public async Task<IActionResult> Suggest(string? q, CancellationToken ct)
    {
        var result = await _search.SuggestAsync(q ?? string.Empty, 8, ct);
        return PartialView("_SearchSuggest", result);
    }

    /// <summary>URL SEO phẳng: phân giải slug → trang danh mục hoặc PDP.</summary>
    [HttpGet("/{slug}")]
    public async Task<IActionResult> Resolve(string slug, CancellationToken ct)
    {
        var match = await _resolver.ResolveAsync(slug, ct);
        switch (match.Kind)
        {
            case SlugKind.Category:
                var category = await _catalog.GetCategoryPageAsync(slug, _region.RegionId, 24, ct);
                return category is null ? NotFound() : View("Category", category);

            case SlugKind.Variant:
                var pdp = await _catalog.GetByVariantSlugAsync(slug, _region.RegionId, ct);
                return pdp is null ? NotFound() : View("ProductDetail", pdp);

            // TODO (vibe-code): Article / Page sẽ render ở Phase 4 (CMS)
            default:
                return NotFound();
        }
    }
}
