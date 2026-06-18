using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Checkout;
using HtMobile.Application.Features.Customers;
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
    private readonly AddressService _addresses;
    private readonly ICurrentUser _user;

    public CheckoutController(CheckoutService checkout, CartContext ctx, AddressService addresses, ICurrentUser user)
    {
        _checkout = checkout;
        _ctx = ctx;
        _addresses = addresses;
        _user = user;
    }

    [HttpGet("/checkout")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var cart = await _checkout.GetSummaryAsync(_ctx.GetOwner(), ct);
        if (cart.IsEmpty) return Redirect("/cart");

        var vm = new CheckoutVm();
        // Khách đăng nhập có địa chỉ mặc định → tự điền form nhận hàng.
        if (_user.UserId is long customerId && await _addresses.GetDefaultAsync(customerId, ct) is { } addr)
        {
            vm.FullName = addr.Recipient;
            vm.Phone = addr.Phone;
            vm.Address = addr.AddressLine;
        }

        ViewBag.Cart = cart;
        return View(vm);
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
