using HtMobile.Application.Common.Interfaces;
using HtMobile.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.ViewComponents;

/// <summary>Badge số lượng trên icon giỏ ở header (SPEC §9). Đếm theo chủ giỏ hiện tại.</summary>
public class CartBadgeViewComponent : ViewComponent
{
    private readonly ICartService _cart;
    private readonly CartContext _ctx;

    public CartBadgeViewComponent(ICartService cart, CartContext ctx)
    {
        _cart = cart;
        _ctx = ctx;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var count = await _cart.GetCountAsync(_ctx.GetOwner());
        return View(count);
    }
}
