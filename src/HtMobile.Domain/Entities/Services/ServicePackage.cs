using HtMobile.Domain.Common;

namespace HtMobile.Domain.Entities.Services;

/// <summary>Gói dịch vụ bảo hành (tiêu chuẩn, kim cương, VIP, mở rộng, AppleCare). SPEC §1, §4.2.</summary>
public class ServicePackage : BaseAuditableEntity
{
    public string Type { get; set; } = string.Empty;   // warranty / applecare / extended…
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMonths { get; set; }
    public decimal Price { get; set; }
}
