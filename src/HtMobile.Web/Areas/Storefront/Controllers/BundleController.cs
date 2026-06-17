using HtMobile.Application.Features.Bundles;
using HtMobile.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Storefront.Controllers;

/// <summary>Thêm combo "mua kèm phụ kiện" vào giỏ (P3-01). Giá mua kèm lấy từ DB trong BundleService.</summary>
[Area("Storefront")]
public class BundleController : Controller
{
    private readonly BundleService _bundle;
    private readonly CartContext _ctx;

    public BundleController(BundleService bundle, CartContext ctx)
    {
        _bundle = bundle;
        _ctx = ctx;
    }

    [HttpPost("/bundle/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add([FromForm] long mainVariantId, [FromForm] long[]? accessoryVariantIds, CancellationToken ct)
    {
        await _bundle.AddToCartAsync(
            _ctx.GetOwner(createGuestIfMissing: true),
            mainVariantId,
            accessoryVariantIds ?? Array.Empty<long>(),
            ct);
        return Redirect("/cart");
    }
}
