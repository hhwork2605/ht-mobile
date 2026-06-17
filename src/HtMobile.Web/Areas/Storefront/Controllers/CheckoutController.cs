using HtMobile.Application.Features.Checkout;
using HtMobile.Web.Infrastructure;
using HtMobile.Web.Models.Checkout;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Storefront.Controllers;

/// <summary>Checkout Phase 2 (COD): form nhận hàng → tạo đơn → màn thành công. Guest hoặc user đều đặt được.</summary>
[Area("Storefront")]
public class CheckoutController : Controller
{
    private readonly CheckoutService _checkout;
    private readonly CartContext _ctx;

    public CheckoutController(CheckoutService checkout, CartContext ctx)
    {
        _checkout = checkout;
        _ctx = ctx;
    }

    [HttpGet("/checkout")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var cart = await _checkout.GetSummaryAsync(_ctx.GetOwner(), ct);
        if (cart.IsEmpty) return Redirect("/cart");

        ViewBag.Cart = cart;
        return View(new CheckoutVm());
    }

    [HttpPost("/checkout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutVm vm, CancellationToken ct)
    {
        var owner = _ctx.GetOwner();
        var cart = await _checkout.GetSummaryAsync(owner, ct);
        if (cart.IsEmpty) return Redirect("/cart");

        if (!ModelState.IsValid)
        {
            ViewBag.Cart = cart;
            return View(vm);
        }

        var shipLabel = vm.ShipMethod == "store" ? "Nhận tại cửa hàng" : "Giao tận nơi";
        var address = $"{vm.FullName} · {vm.Phone} · {vm.Address} ({shipLabel})";

        var result = await _checkout.PlaceOrderAsync(owner, address, ct);
        if (result is null) return Redirect("/cart");

        return RedirectToAction(nameof(Success), new { code = result.Code });
    }

    [HttpGet("/checkout/success")]
    public IActionResult Success(string? code)
    {
        if (string.IsNullOrEmpty(code)) return Redirect("/");
        ViewBag.Code = code;
        return View();
    }
}
