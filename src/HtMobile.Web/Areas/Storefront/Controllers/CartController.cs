using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Storefront.Controllers;

[Area("Storefront")]
public class CartController : Controller
{
    // TODO (vibe-code Phase 2): giỏ hàng (guest theo session + user), cập nhật SL.
    [HttpGet("/cart")]
    public IActionResult Index() => View();
}
