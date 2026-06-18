using HtMobile.Application.Features.Catalog;
using HtMobile.Application.Features.Catalog.Dtos;
using HtMobile.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
public class CategoriesController : ControllerBase
{
    private readonly AdminProductService _products;
    private readonly AdminCategoryService _categories;

    public CategoriesController(AdminProductService products, AdminCategoryService categories)
    {
        _products = products;
        _categories = categories;
    }

    /// <summary>Tuỳ chọn danh mục (id, name) cho dropdown — dùng ở form sản phẩm + chọn cha danh mục.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminCategoryOption>>> Options(CancellationToken ct)
        => Ok(await _products.GetCategoryOptionsAsync(ct));

    /// <summary>Danh sách danh mục đầy đủ (quản lý).</summary>
    [HttpGet("manage")]
    public async Task<ActionResult<IReadOnlyList<AdminCategoryRow>>> List(CancellationToken ct)
        => Ok(await _categories.GetListAsync(ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<AdminCategoryDetail>> Get(long id, CancellationToken ct)
    {
        var dto = await _categories.GetAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryInput input, CancellationToken ct)
    {
        var (result, id) = await _categories.CreateAsync(input, ct);
        return result switch
        {
            AdminCategoryResult.Ok => CreatedAtAction(nameof(Get), new { id }, new { id }),
            AdminCategoryResult.SlugExists => Conflict(new { message = "Slug đã tồn tại." }),
            AdminCategoryResult.InvalidParent => BadRequest(new { message = "Danh mục cha không hợp lệ." }),
            _ => BadRequest(),
        };
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] CategoryInput input, CancellationToken ct)
        => Map(await _categories.UpdateAsync(id, input, ct));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
        => Map(await _categories.DeleteAsync(id, ct));

    private IActionResult Map(AdminCategoryResult r) => r switch
    {
        AdminCategoryResult.Ok => NoContent(),
        AdminCategoryResult.NotFound => NotFound(),
        AdminCategoryResult.SlugExists => Conflict(new { message = "Slug đã tồn tại." }),
        AdminCategoryResult.InvalidParent => BadRequest(new { message = "Danh mục cha không hợp lệ (trùng chính nó hoặc tạo vòng lặp)." }),
        AdminCategoryResult.HasChildren => Conflict(new { message = "Không thể xoá: danh mục còn danh mục con." }),
        AdminCategoryResult.HasProducts => Conflict(new { message = "Không thể xoá: danh mục còn sản phẩm." }),
        _ => BadRequest(),
    };
}
