using HtMobile.Application.Features.Orders;
using HtMobile.Application.Features.Orders.Dtos;
using HtMobile.Domain.Constants;
using HtMobile.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.Admin)]
[Route("admin/orders")]
public class OrdersController : Controller
{
    private readonly AdminOrderService _orders;

    public OrdersController(AdminOrderService orders) => _orders = orders;

    [HttpGet("")]
    public async Task<IActionResult> Index(string? status, CancellationToken ct)
    {
        OrderStatus? filter = Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var s) ? s : null;
        var dto = await _orders.GetListAsync(filter, ct);
        return View(dto);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var dto = await _orders.GetDetailAsync(id, ct);
        return dto is null ? NotFound() : View(dto);
    }

    [HttpPost("{id:long}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(long id, OrderStatus status, CancellationToken ct)
    {
        var result = await _orders.ChangeStatusAsync(id, status, ct);
        if (result == ChangeStatusResult.NotFound) return NotFound();
        if (result == ChangeStatusResult.InvalidTransition)
            TempData["StatusError"] = "Không thể chuyển sang trạng thái đã chọn.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
