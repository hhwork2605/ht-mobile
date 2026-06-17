using HtMobile.Application.Features.Catalog;
using HtMobile.Application.Features.Catalog.Dtos;
using HtMobile.Domain.Constants;
using HtMobile.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.Admin)]
[Route("admin/products")]
public class ProductsController : Controller
{
    private readonly AdminProductService _products;

    public ProductsController(AdminProductService products) => _products = products;

    [HttpGet("")]
    public async Task<IActionResult> Index(long? category, CancellationToken ct)
        => View(await _products.GetListAsync(category, ct));

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken ct)
        => View(new ProductCreateVm { Categories = await _products.GetCategoryOptionsAsync(ct) });

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            vm.Categories = await _products.GetCategoryOptionsAsync(ct);
            return View(vm);
        }

        var (result, id) = await _products.CreateAsync(
            new ProductInput(vm.Name, vm.Slug, vm.CategoryId, vm.Brand, vm.Tagline, vm.Description),
            new VariantInput(vm.Sku, vm.Storage, vm.Color, vm.BasePrice, vm.CompareAtPrice, vm.Status),
            ct);

        if (result == AdminProductResult.SlugExists)
        {
            ModelState.AddModelError(nameof(vm.Slug), "Slug đã tồn tại — đổi tên hoặc nhập slug khác.");
            vm.Categories = await _products.GetCategoryOptionsAsync(ct);
            return View(vm);
        }
        if (result == AdminProductResult.SkuExists)
        {
            ModelState.AddModelError(nameof(vm.Sku), "SKU đã tồn tại — nhập SKU khác.");
            vm.Categories = await _products.GetCategoryOptionsAsync(ct);
            return View(vm);
        }

        TempData["Flash"] = "Đã tạo sản phẩm.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpGet("{id:long}/edit")]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var dto = await _products.GetEditAsync(id, ct);
        return dto is null ? NotFound() : View(ToEditVm(dto));
    }

    [HttpPost("{id:long}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, ProductEditVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            vm.Categories = await _products.GetCategoryOptionsAsync(ct);
            return View(vm);
        }

        var result = await _products.UpdateAsync(id,
            new ProductInput(vm.Name, vm.Slug, vm.CategoryId, vm.Brand, vm.Tagline, vm.Description),
            vm.Variants.Select(v => new VariantEdit(v.Id, v.BasePrice, v.CompareAtPrice, v.Status)).ToList(),
            ct);

        if (result == AdminProductResult.NotFound) return NotFound();
        if (result == AdminProductResult.SlugExists)
        {
            ModelState.AddModelError(nameof(vm.Slug), "Slug đã tồn tại — đổi tên hoặc nhập slug khác.");
            vm.Categories = await _products.GetCategoryOptionsAsync(ct);
            return View(vm);
        }

        TempData["Flash"] = "Đã lưu thay đổi.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    // ===== AJAX (htmx) — trả partial cập nhật tại chỗ =====

    /// <summary>Thêm biến thể: trả 1 hàng partial append vào bảng biến thể (hx-swap=beforeend).</summary>
    [HttpPost("{id:long}/variants")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVariant(long id, VariantAddVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

        var result = await _products.AddVariantAsync(id,
            new VariantInput(vm.Sku, vm.Storage, vm.Color, vm.BasePrice, vm.CompareAtPrice, vm.Status), ct);
        if (result == AdminProductResult.NotFound) return NotFound();
        if (result == AdminProductResult.SkuExists) return BadRequest("SKU đã tồn tại — nhập SKU khác.");

        var dto = await _products.GetEditAsync(id, ct);
        var index = dto!.Variants.Count - 1;        // biến thể vừa thêm = hàng cuối
        return PartialView("_VariantRow", (dto.Variants[index], index, id));
    }

    /// <summary>Ẩn/hiện biến thể: trả lại hàng partial đã đổi trạng thái (hx-swap=outerHTML).</summary>
    [HttpPost("variants/{variantId:long}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleVariant(long variantId, CancellationToken ct)
    {
        var (result, productId) = await _products.ToggleVariantAsync(variantId, ct);
        if (result == AdminProductResult.NotFound) return NotFound();

        var dto = await _products.GetEditAsync(productId, ct);
        var index = dto?.Variants.ToList().FindIndex(v => v.Id == variantId) ?? -1;
        if (dto is null || index < 0) return NotFound();
        return PartialView("_VariantRow", (dto.Variants[index], index, productId));
    }

    private static ProductEditVm ToEditVm(AdminProductEditDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Slug = dto.Slug,
        CategoryId = dto.CategoryId,
        Brand = dto.Brand,
        Tagline = dto.Tagline,
        Description = dto.Description,
        Categories = dto.Categories,
        Variants = dto.Variants.Select(v => new VariantEditVm
        {
            Id = v.Id,
            Sku = v.Sku,
            Storage = v.Storage,
            Color = v.Color,
            BasePrice = v.BasePrice,
            CompareAtPrice = v.CompareAtPrice,
            Status = v.Status
        }).ToList()
    };
}
