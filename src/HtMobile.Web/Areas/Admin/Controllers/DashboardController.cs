using HtMobile.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.Admin)]
public class DashboardController : Controller
{
    // TODO (vibe-code Phase 5): quản lý SP/đơn/khuyến mãi/CMS/báo cáo.
    public IActionResult Index() => View();
}
