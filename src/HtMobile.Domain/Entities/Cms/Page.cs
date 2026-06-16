using HtMobile.Domain.Common;

namespace HtMobile.Domain.Entities.Cms;

/// <summary>Trang nội dung tĩnh/CMS (chính sách, giao hàng, đổi trả…). SPEC §3.</summary>
public class Page : BaseAuditableEntity
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
}
