using HtMobile.Application.Features.Orders;
using HtMobile.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Storefront.Controllers;

/// <summary>Tra cứu đơn hàng cho khách vãng lai (COD) bằng mã đơn + SĐT — không cần đăng nhập.</summary>
[Area("Storefront")]
public class OrderLookupController : Controller
{
    private readonly OrderHistoryService _orders;

    public OrderLookupController(OrderHistoryService orders) => _orders = orders;

    [HttpGet("/tra-cuu-don")]
    public IActionResult Index() => View(new OrderLookupVm());

    [HttpPost("/tra-cuu-don")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(OrderLookupVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        vm.Searched = true;
        vm.Result = await _orders.LookupAsync(vm.Code!, vm.Phone!, ct);
        return View(vm);
    }
}
