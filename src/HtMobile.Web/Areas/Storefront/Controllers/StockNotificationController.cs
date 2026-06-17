using HtMobile.Application.Features.StockNotifications;
using HtMobile.Web.Models.Stock;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Storefront.Controllers;

/// <summary>Theo dõi hàng về (P3-04): đăng ký nhận thông báo khi variant hết hàng có lại. htmx → partial.</summary>
[Area("Storefront")]
public class StockNotificationController : Controller
{
    private readonly StockNotificationService _stock;

    public StockNotificationController(StockNotificationService stock) => _stock = stock;

    [HttpPost("/stock-notify")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Notify([FromForm] long variantId, [FromForm] string? contact, CancellationToken ct)
    {
        var result = await _stock.SubscribeAsync(variantId, contact, ct);
        return PartialView("_StockNotify", new StockNotifyVm { VariantId = variantId, Contact = contact, Result = result });
    }
}
