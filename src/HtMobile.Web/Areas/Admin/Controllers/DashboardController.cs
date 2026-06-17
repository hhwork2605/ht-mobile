using HtMobile.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.Admin)]
[Route("admin")]
public class DashboardController : Controller
{
    // TODO (vibe-code Phase 5): quản lý SP/đơn/khuyến mãi/CMS/báo cáo.
    // /admin + /admin/dashboard đều về bảng điều khiển (layout link tới /admin).
    [HttpGet("")]
    [HttpGet("dashboard")]
    public IActionResult Index() => View();
}
