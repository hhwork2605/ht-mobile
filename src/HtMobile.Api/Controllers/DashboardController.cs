using HtMobile.Application.Features.Dashboard;
using HtMobile.Application.Features.Dashboard.Dtos;
using HtMobile.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboard;

    public DashboardController(DashboardService dashboard) => _dashboard = dashboard;

    [HttpGet]
    public async Task<ActionResult<DashboardStatsDto>> Get(CancellationToken ct)
        => Ok(await _dashboard.GetAsync(ct));
}
