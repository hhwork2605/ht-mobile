using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Cart;

namespace HtMobile.Web.Infrastructure;

/// <summary>
/// Xác định chủ giỏ ở tầng Web: user đăng nhập (UserId) hoặc khách vãng lai (cookie GUID <c>htm_cart</c>).
/// Cookie chỉ được tạo khi thực sự cần (lúc thêm vào giỏ) để tránh set cookie cho khách chỉ lướt xem.
/// </summary>
public class CartContext
{
    public const string GuestCookie = "htm_cart";

    private readonly IHttpContextAccessor _http;
    private readonly ICurrentUser _user;

    public CartContext(IHttpContextAccessor http, ICurrentUser user)
    {
        _http = http;
        _user = user;
    }

    /// <summary>Lấy owner. <paramref name="createGuestIfMissing"/>=true sẽ tạo cookie guid cho khách vãng lai.</summary>
    public CartOwner GetOwner(bool createGuestIfMissing = false)
    {
        if (_user.IsAuthenticated && _user.UserId is long uid)
            return new CartOwner(uid, null);

        var ctx = _http.HttpContext!;
        var id = ctx.Request.Cookies[GuestCookie];
        if (string.IsNullOrWhiteSpace(id))
        {
            if (!createGuestIfMissing) return new CartOwner(null, null);
            id = Guid.NewGuid().ToString("N");
            ctx.Response.Cookies.Append(GuestCookie, id, new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = ctx.Request.IsHttps,
                MaxAge = TimeSpan.FromDays(30)
            });
        }
        return new CartOwner(null, id);
    }

    /// <summary>Đọc session id của giỏ guest từ cookie (null nếu chưa có) — dùng để merge khi đăng nhập.</summary>
    public string? GuestSessionId => _http.HttpContext?.Request.Cookies[GuestCookie];

    /// <summary>Xoá cookie giỏ guest (sau khi đã merge vào giỏ user).</summary>
    public void ClearGuest() => _http.HttpContext?.Response.Cookies.Delete(GuestCookie);
}
