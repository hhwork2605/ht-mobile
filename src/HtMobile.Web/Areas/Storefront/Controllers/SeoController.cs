using System.Text;
using HtMobile.Application.Features.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Storefront.Controllers;

/// <summary>Sitemap XML cho bộ máy tìm kiếm (docs/conventions.md §SEO). robots.txt phục vụ tĩnh ở wwwroot.</summary>
[Area("Storefront")]
public class SeoController : Controller
{
    private readonly CatalogService _catalog;

    public SeoController(CatalogService catalog) => _catalog = catalog;

    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Sitemap(CancellationToken ct)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var entries = await _catalog.GetSitemapEntriesAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // Trang chủ
        sb.AppendLine($"  <url><loc>{baseUrl}/</loc><changefreq>daily</changefreq><priority>1.0</priority></url>");

        foreach (var e in entries)
        {
            var loc = $"{baseUrl}/{Uri.EscapeDataString(e.Slug)}";
            var lastmod = e.LastModified is { } d ? $"<lastmod>{d:yyyy-MM-dd}</lastmod>" : string.Empty;
            sb.AppendLine($"  <url><loc>{loc}</loc>{lastmod}</url>");
        }

        sb.AppendLine("</urlset>");
        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }
}
