using HtMobile.Application.Features.Seo;
using HtMobile.Application.Features.Seo.Dtos;
using HtMobile.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Api.Controllers;

[ApiController]
[Route("api/seo")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
public class SeoController : ControllerBase
{
    private readonly AdminSeoService _seo;

    public SeoController(AdminSeoService seo) => _seo = seo;

    /// <summary>Tổng quan & kiểm tra SEO catalog (đếm sitemap + mục thiếu SEO).</summary>
    [HttpGet("overview")]
    public async Task<ActionResult<SeoOverviewDto>> Overview(CancellationToken ct)
        => Ok(await _seo.GetOverviewAsync(ct));
}
