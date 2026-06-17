using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.TradeIn;
using HtMobile.Web.Models.TradeIn;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Storefront.Controllers;

/// <summary>Thu cũ đổi mới (P3-03): form định giá + gửi yêu cầu. Estimate tính lại server-side trong TradeInService.</summary>
[Area("Storefront")]
public class TradeInController : Controller
{
    private readonly TradeInService _trade;
    private readonly ICurrentUser _user;

    public TradeInController(TradeInService trade, ICurrentUser user)
    {
        _trade = trade;
        _user = user;
    }

    [HttpGet("/thu-cu-doi-moi")]
    public IActionResult Index() => View(new TradeInVm());

    [HttpPost("/thu-cu-doi-moi")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(TradeInVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _trade.SubmitAsync(_user.UserId, vm.DeviceKey, vm.ConditionKey, ct);
        if (result is null)
        {
            ModelState.AddModelError(string.Empty, "Dòng máy hoặc tình trạng không hợp lệ.");
            return View(vm);
        }

        ViewBag.Result = result;
        return View("Submitted");
    }
}
