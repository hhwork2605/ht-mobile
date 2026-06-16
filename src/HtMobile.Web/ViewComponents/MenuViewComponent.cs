using HtMobile.Application.Features.Catalog;
using HtMobile.Application.Features.Catalog.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.ViewComponents;

/// <summary>Thanh điều hướng danh mục (SPEC §9 — CategoryNav/MegaMenu). Tái dùng trong Header.</summary>
public class MenuViewComponent : ViewComponent
{
    private readonly CatalogService _catalog;

    public MenuViewComponent(CatalogService catalog) => _catalog = catalog;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        IReadOnlyList<CategoryDto> categories = await _catalog.GetMenuAsync();
        return View(categories);
    }
}
