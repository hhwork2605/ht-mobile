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
    private readonly AdminProductImageService _images;

    public ProductsController(AdminProductService products, AdminProductImageService images)
    {
        _products = products;
        _images = images;
    }

    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    { "image/jpeg", "image/png", "image/webp", "image/gif" };
    private const long MaxImageBytes = 5 * 1024 * 1024;   // 5MB

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
            AdminProductResult.InvalidSpecs => BadRequest(new { field = "specs", message = "Thông số kỹ thuật không phải JSON hợp lệ." }),
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
            AdminProductResult.InvalidSpecs => BadRequest(new { field = "specs", message = "Thông số kỹ thuật không phải JSON hợp lệ." }),
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

    // ===== Ảnh sản phẩm (gắn ở model) =====

    [HttpGet("{id:long}/images")]
    public async Task<ActionResult<IReadOnlyList<ProductImageRow>>> Images(long id, CancellationToken ct)
        => Ok(await _images.ListAsync(id, ct));

    [HttpPost("{id:long}/images")]
    [RequestSizeLimit(MaxImageBytes + 1024)]
    public async Task<IActionResult> UploadImage(long id, IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { field = "file", message = "Chưa chọn tệp ảnh." });
        if (file.Length > MaxImageBytes)
            return BadRequest(new { field = "file", message = "Ảnh vượt quá 5MB." });
        if (!AllowedImageTypes.Contains(file.ContentType))
            return BadRequest(new { field = "file", message = "Chỉ chấp nhận ảnh JPEG/PNG/WebP/GIF." });

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        await using var stream = file.OpenReadStream();
        var (result, row) = await _images.AddAsync(id, stream, file.FileName, baseUrl, ct);
        return result == AdminProductResult.NotFound ? NotFound() : Ok(row);
    }

    [HttpDelete("images/{imageId:long}")]
    public async Task<IActionResult> DeleteImage(long imageId, CancellationToken ct)
    {
        var result = await _images.DeleteAsync(imageId, ct);
        return result == AdminProductResult.NotFound ? NotFound() : NoContent();
    }

    [HttpPut("{id:long}/images/order")]
    public async Task<IActionResult> ReorderImages(long id, [FromBody] long[] orderedIds, CancellationToken ct)
    {
        var result = await _images.ReorderAsync(id, orderedIds ?? Array.Empty<long>(), ct);
        return result == AdminProductResult.NotFound ? NotFound() : NoContent();
    }
}
