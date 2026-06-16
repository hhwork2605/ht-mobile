using HtMobile.Domain.Common;

namespace HtMobile.Domain.Entities.Cms;

/// <summary>
/// Slide banner trang chủ (Carousel — SPEC §9). Quản trị CRUD ở Admin (Phase 5).
/// Hiển thị khi <see cref="IsActive"/> và thời điểm xem nằm trong cửa sổ <see cref="StartsAt"/>..<see cref="EndsAt"/>.
/// </summary>
public class Banner : BaseAuditableEntity
{
    /// <summary>Nhãn nhỏ phía trên tiêu đề, vd "Vừa ra mắt".</summary>
    public string? Eyebrow { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? ImageUrl { get; set; }

    /// <summary>Đích khi bấm CTA (slug nội bộ, vd <c>/iphone</c>).</summary>
    public string? LinkUrl { get; set; }
    public string? CtaText { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Cửa sổ hiển thị (tùy chọn). Null = không giới hạn ở đầu/cuối tương ứng.</summary>
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }

    /// <summary>Banner có được hiển thị tại thời điểm <paramref name="at"/> không.</summary>
    public bool IsActiveAt(DateTime at)
        => IsActive
           && (StartsAt is null || at >= StartsAt)
           && (EndsAt is null || at <= EndsAt);
}
