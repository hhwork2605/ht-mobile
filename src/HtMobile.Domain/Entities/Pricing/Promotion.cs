using HtMobile.Domain.Common;
using HtMobile.Domain.Enums;

namespace HtMobile.Domain.Entities.Pricing;

/// <summary>Khuyến mãi nhiều tầng. Điều kiện áp dụng lưu jsonb (ConditionsJson). SPEC §4.2.</summary>
public class Promotion : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Mã giảm giá (voucher). <c>null</c> = KM tự động (auto-apply theo điều kiện ở pricing engine);
    /// có giá trị = chỉ áp khi khách NHẬP đúng mã ở giỏ/checkout (KHÔNG auto-apply). UNIQUE khi khác null.
    /// </summary>
    public string? Code { get; set; }

    public PromotionType Type { get; set; }
    public decimal Value { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }

    /// <summary>Điều kiện áp dụng (danh mục, SP, ngưỡng đơn tối thiểu…) dạng jsonb.</summary>
    public string? ConditionsJson { get; set; }

    public bool IsActiveAt(DateTime at) => at >= StartsAt && at <= EndsAt;
}
