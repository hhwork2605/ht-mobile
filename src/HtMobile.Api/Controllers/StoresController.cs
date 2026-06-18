using HtMobile.Application.Features.Inventory;
using HtMobile.Application.Features.Inventory.Dtos;
using HtMobile.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Api.Controllers;

[ApiController]
[Route("api/stores")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
public class StoresController : ControllerBase
{
    private readonly AdminInventoryService _inv;

    public StoresController(AdminInventoryService inv) => _inv = inv;

    public record StockUpdateRequest(List<StockUpdateItem> Items);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StoreRow>>> List(CancellationToken ct)
        => Ok(await _inv.GetStoresAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StoreInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) return BadRequest(new { message = "Tên cửa hàng là bắt buộc." });
        var id = await _inv.CreateStoreAsync(input, ct);
        return Created($"/api/stores/{id}", new { id });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] StoreInput input, CancellationToken ct)
        => Map(await _inv.UpdateStoreAsync(id, input, ct));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
        => Map(await _inv.DeleteStoreAsync(id, ct));

    [HttpGet("{id:long}/stock")]
    public async Task<ActionResult<IReadOnlyList<StockRow>>> Stock(long id, CancellationToken ct)
        => Ok(await _inv.GetStockAsync(id, ct));

    [HttpPut("{id:long}/stock")]
    public async Task<IActionResult> SetStock(long id, [FromBody] StockUpdateRequest req, CancellationToken ct)
        => Map(await _inv.SetStockAsync(id, req.Items, ct));

    private IActionResult Map(AdminInventoryResult r) => r switch
    {
        AdminInventoryResult.Ok => NoContent(),
        AdminInventoryResult.NotFound => NotFound(),
        AdminInventoryResult.HasStock => Conflict(new { message = "Không thể xoá: cửa hàng còn tồn kho > 0." }),
        _ => BadRequest(),
    };
}
