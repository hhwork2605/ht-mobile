using HtMobile.Application.Features.Pricing;
using HtMobile.Application.Features.Pricing.Dtos;
using HtMobile.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Api.Controllers;

[ApiController]
[Route("api/promotions")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
public class PromotionsController : ControllerBase
{
    private readonly AdminPromotionService _promos;

    public PromotionsController(AdminPromotionService promos) => _promos = promos;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminPromotionRow>>> List(CancellationToken ct)
        => Ok(await _promos.GetListAsync(ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<AdminPromotionDetail>> Get(long id, CancellationToken ct)
    {
        var dto = await _promos.GetAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PromotionInput input, CancellationToken ct)
    {
        if (input.EndsAt < input.StartsAt) return BadRequest(new { message = "Ngày kết thúc phải sau ngày bắt đầu." });
        var (result, id) = await _promos.CreateAsync(input, ct);
        return result switch
        {
            PromotionWriteResult.Ok => CreatedAtAction(nameof(Get), new { id }, new { id }),
            PromotionWriteResult.CodeExists => Conflict(new { field = "code", message = "Mã giảm giá đã tồn tại." }),
            _ => BadRequest()
        };
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] PromotionInput input, CancellationToken ct)
    {
        if (input.EndsAt < input.StartsAt) return BadRequest(new { message = "Ngày kết thúc phải sau ngày bắt đầu." });
        var result = await _promos.UpdateAsync(id, input, ct);
        return result switch
        {
            PromotionWriteResult.Ok => NoContent(),
            PromotionWriteResult.NotFound => NotFound(),
            PromotionWriteResult.CodeExists => Conflict(new { field = "code", message = "Mã giảm giá đã tồn tại." }),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
        => await _promos.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
