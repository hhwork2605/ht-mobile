using HtMobile.Application.Features.Catalog;
using HtMobile.Application.Features.Catalog.Dtos;
using HtMobile.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
public class ProductsController : ControllerBase
{
    private readonly AdminProductService _products;

    public ProductsController(AdminProductService products) => _products = products;

    public record ProductCreateRequest(ProductInput Product, VariantInput Variant);
    public record ProductUpdateRequest(ProductInput Product, List<VariantEdit> Variants);

    [HttpGet]
    public async Task<ActionResult<AdminProductListDto>> List([FromQuery] long? category, CancellationToken ct)
        => Ok(await _products.GetListAsync(category, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<AdminProductEditDto>> Get(long id, CancellationToken ct)
    {
        var dto = await _products.GetEditAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductCreateRequest req, CancellationToken ct)
    {
        var (result, id) = await _products.CreateAsync(req.Product, req.Variant, ct);
        return result switch
        {
            AdminProductResult.Ok => CreatedAtAction(nameof(Get), new { id }, new { id }),
            AdminProductResult.SlugExists => Conflict(new { field = "slug", message = "Slug đã tồn tại." }),
            AdminProductResult.SkuExists => Conflict(new { field = "sku", message = "SKU đã tồn tại." }),
            _ => BadRequest()
        };
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] ProductUpdateRequest req, CancellationToken ct)
    {
        var result = await _products.UpdateAsync(id, req.Product, req.Variants, ct);
        return result switch
        {
            AdminProductResult.Ok => NoContent(),
            AdminProductResult.NotFound => NotFound(),
            AdminProductResult.SlugExists => Conflict(new { field = "slug", message = "Slug đã tồn tại." }),
            _ => BadRequest()
        };
    }

    [HttpPost("{id:long}/variants")]
    public async Task<IActionResult> AddVariant(long id, [FromBody] VariantInput input, CancellationToken ct)
    {
        var result = await _products.AddVariantAsync(id, input, ct);
        return result switch
        {
            AdminProductResult.Ok => NoContent(),
            AdminProductResult.NotFound => NotFound(),
            AdminProductResult.SkuExists => Conflict(new { field = "sku", message = "SKU đã tồn tại." }),
            _ => BadRequest()
        };
    }

    [HttpPost("variants/{variantId:long}/toggle")]
    public async Task<IActionResult> ToggleVariant(long variantId, CancellationToken ct)
    {
        var (result, productId) = await _products.ToggleVariantAsync(variantId, ct);
        return result == AdminProductResult.NotFound ? NotFound() : Ok(new { productId });
    }
}

[ApiController]
[Route("api/categories")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
public class CategoriesController : ControllerBase
{
    private readonly AdminProductService _products;

    public CategoriesController(AdminProductService products) => _products = products;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminCategoryOption>>> List(CancellationToken ct)
        => Ok(await _products.GetCategoryOptionsAsync(ct));
}
