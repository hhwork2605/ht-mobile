using HtMobile.Application.Features.Reports;
using HtMobile.Application.Features.Reports.Dtos;
using HtMobile.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
public class ReportsController : ControllerBase
{
    private readonly ReportsService _reports;

    public ReportsController(ReportsService reports) => _reports = reports;

    /// <summary>Báo cáo bán hàng theo khoảng ngày. Mặc định 30 ngày gần nhất.</summary>
    [HttpGet("sales")]
    public async Task<ActionResult<SalesReportDto>> Sales([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var toDate = to ?? DateTime.Now;
        var fromDate = from ?? toDate.AddDays(-29);
        if (fromDate > toDate) (fromDate, toDate) = (toDate, fromDate);
        return Ok(await _reports.GetSalesAsync(fromDate, toDate, 10, ct));
    }
}
