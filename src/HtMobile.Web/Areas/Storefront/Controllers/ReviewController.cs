using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Catalog;
using HtMobile.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Storefront.Controllers;

/// <summary>Gửi đánh giá sản phẩm (yêu cầu đăng nhập tài khoản Customer). Mỗi khách 1 đánh giá/model (gửi lại = cập nhật).</summary>
[Area("Storefront")]
public class ReviewController : Controller
{
    private readonly ReviewService _reviews;
    private readonly ICurrentUser _user;

    public ReviewController(ReviewService reviews, ICurrentUser user)
    {
        _reviews = reviews;
        _user = user;
    }

    [Authorize]
    [HttpPost("/review")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(long productId, int rating, string? content, string? returnSlug, CancellationToken ct)
    {
        if (_user.UserId is not long customerId) return Redirect("/login");

        var result = await _reviews.AddOrUpdateAsync(productId, customerId, rating, content, ct);
        TempData["ReviewMsg"] = result switch
        {
            ReviewResult.Ok => "Cảm ơn bạn đã đánh giá!",
            ReviewResult.InvalidRating => "Vui lòng chọn số sao (1–5).",
            _ => "Không gửi được đánh giá."
        };

        var slug = UrlSafety.SafeLocalUrl(string.IsNullOrWhiteSpace(returnSlug) ? "/" : "/" + returnSlug.TrimStart('/'));
        return Redirect(slug + "#reviews");
    }
}
