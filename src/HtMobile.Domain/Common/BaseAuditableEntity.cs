namespace HtMobile.Domain.Common;

/// <summary>Entity có audit thời gian (UTC, lưu dạng timestamp không tz). Gán tự động qua interceptor ở Infrastructure.</summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
