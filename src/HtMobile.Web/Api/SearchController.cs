using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Api;

/// <summary>API autocomplete (SPEC §8): GET /api/search/suggest?q= — trả JSON.</summary>
[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _search;

    public SearchController(ISearchService search) => _search = search;

    [HttpGet("suggest")]
    public async Task<ActionResult<ApiResponse<SearchSuggestResult>>> Suggest([FromQuery] string? q, CancellationToken ct)
    {
        var result = await _search.SuggestAsync(q ?? string.Empty, 8, ct);
        return Ok(ApiResponse<SearchSuggestResult>.Ok(result));
    }
}
