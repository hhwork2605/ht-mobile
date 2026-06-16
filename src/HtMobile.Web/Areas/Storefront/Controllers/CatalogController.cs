using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Catalog;
using HtMobile.Web.Infrastructure.Routing;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Storefront.Controllers;

[Area("Storefront")]
public class CatalogController : Controller
{
    private readonly CatalogService _catalog;
    private readonly ISlugResolver _resolver;
    private readonly ISearchService _search;

    public CatalogController(CatalogService catalog, ISlugResolver resolver, ISearchService search)
    {
        _catalog = catalog;
        _resolver = resolver;
        _search = search;
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
                var category = await _catalog.GetCategoryPageAsync(slug, 24, ct);
                return category is null ? NotFound() : View("Category", category);

            case SlugKind.Variant:
                var pdp = await _catalog.GetByVariantSlugAsync(slug, ct);
                return pdp is null ? NotFound() : View("ProductDetail", pdp);

            // TODO (vibe-code): Article / Page sẽ render ở Phase 4 (CMS)
            default:
                return NotFound();
        }
    }
}
