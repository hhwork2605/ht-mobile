using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Catalog;
using HtMobile.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Storefront.Controllers;

[Area("Storefront")]
public class HomeController : Controller
{
    private readonly CatalogService _catalog;
    private readonly ICurrentRegion _region;

    public HomeController(CatalogService catalog, ICurrentRegion region)
    {
        _catalog = catalog;
        _region = region;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var home = await _catalog.GetHomePageAsync(_region.RegionId, featuredCount: 8, ct);
        return View(home);
    }

    [HttpGet("/error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
}
