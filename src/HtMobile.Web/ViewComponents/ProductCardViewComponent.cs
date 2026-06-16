using HtMobile.Application.Features.Catalog.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.ViewComponents;

/// <summary>Thẻ sản phẩm tái dùng (SPEC §9 — ProductCard). Dùng ở Trang chủ / Danh mục / Search.</summary>
public class ProductCardViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ProductCardDto product) => View(product);
}
