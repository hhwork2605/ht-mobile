using HtMobile.Application.Common.Interfaces;

namespace HtMobile.Web.Infrastructure;

/// <summary>Vùng giá đang chọn, lưu ở cookie. Mặc định miền Bắc (Id = 1 theo seed).</summary>
public class CurrentRegion : ICurrentRegion
{
    public const string CookieName = "regionId";
    public const long DefaultRegionId = 1;

    private readonly IHttpContextAccessor _accessor;

    public CurrentRegion(IHttpContextAccessor accessor) => _accessor = accessor;

    public long RegionId
    {
        get
        {
            var raw = _accessor.HttpContext?.Request.Cookies[CookieName];
            return long.TryParse(raw, out var id) && id > 0 ? id : DefaultRegionId;
        }
    }
}
