using HtMobile.Domain.Common;

namespace HtMobile.Domain.Entities.Localization;

/// <summary>Bản dịch nội dung động (tên/mô tả SP…) theo ngôn ngữ. SPEC §7, đa ngôn ngữ VI/EN.</summary>
public class Translation : BaseEntity
{
    public string Entity { get; set; } = string.Empty;   // "Product", "Category"…
    public long EntityId { get; set; }
    public string Lang { get; set; } = string.Empty;     // "vi" / "en"
    public string Field { get; set; } = string.Empty;    // "Name" / "Description"
    public string Value { get; set; } = string.Empty;
}
