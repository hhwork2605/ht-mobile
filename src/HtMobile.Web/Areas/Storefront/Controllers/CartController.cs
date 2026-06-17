using HtMobile.Application.Common.Interfaces;
using HtMobile.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Storefront.Controllers;

[Area("Storefront")]
public class CartController : Controller
{
    private readonly ICartService _cart;
    private readonly CartContext _ctx;

    public CartController(ICartService cart, CartContext ctx)
    {
        _cart = cart;
        _ctx = ctx;
    }

    [HttpGet("/cart")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var dto = await _cart.GetCartAsync(_ctx.GetOwner(), ct);
        return View(dto);
    }

    [HttpPost("/cart/items")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add([FromForm] long variantId, [FromForm] int quantity = 1, CancellationToken ct = default)
    {
        await _cart.AddItemAsync(_ctx.GetOwner(createGuestIfMissing: true), variantId, quantity, ct: ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/cart/items/{id:long}/inc")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Inc(long id, CancellationToken ct) => MutateAsync(id, +1, ct);

    [HttpPost("/cart/items/{id:long}/dec")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Dec(long id, CancellationToken ct) => MutateAsync(id, -1, ct);

    [HttpPost("/cart/items/{id:long}/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(long id, CancellationToken ct)
    {
        await _cart.RemoveItemAsync(_ctx.GetOwner(), id, ct);
        return await CartBodyAsync(ct);
    }

    private async Task<IActionResult> MutateAsync(long id, int delta, CancellationToken ct)
    {
        await _cart.UpdateQuantityAsync(_ctx.GetOwner(), id, delta, ct);
        return await CartBodyAsync(ct);
    }

    private async Task<IActionResult> CartBodyAsync(CancellationToken ct)
    {
        var dto = await _cart.GetCartAsync(_ctx.GetOwner(), ct);
        return PartialView("_CartBody", dto);
    }
}
