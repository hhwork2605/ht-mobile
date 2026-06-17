using HtMobile.Application.Features.Orders;
using HtMobile.Application.Features.Orders.Dtos;
using HtMobile.Domain.Constants;
using HtMobile.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
public class OrdersController : ControllerBase
{
    private readonly AdminOrderService _orders;

    public OrdersController(AdminOrderService orders) => _orders = orders;

    public record ChangeStatusRequest(OrderStatus Status);

    [HttpGet]
    public async Task<ActionResult<AdminOrderListDto>> List([FromQuery] OrderStatus? status, CancellationToken ct)
        => Ok(await _orders.GetListAsync(status, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<AdminOrderDetailDto>> Get(long id, CancellationToken ct)
    {
        var dto = await _orders.GetDetailAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:long}/status")]
    public async Task<IActionResult> ChangeStatus(long id, [FromBody] ChangeStatusRequest req, CancellationToken ct)
    {
        var result = await _orders.ChangeStatusAsync(id, req.Status, ct);
        return result switch
        {
            ChangeStatusResult.Ok => Ok(await _orders.GetDetailAsync(id, ct)),
            ChangeStatusResult.NotFound => NotFound(),
            ChangeStatusResult.InvalidTransition => Conflict(new { message = "Không thể chuyển sang trạng thái đã chọn." }),
            _ => BadRequest()
        };
    }
}
