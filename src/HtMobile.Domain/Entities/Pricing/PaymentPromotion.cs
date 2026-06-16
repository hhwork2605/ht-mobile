using HtMobile.Domain.Common;

namespace HtMobile.Domain.Entities.Pricing;

/// <summary>Ưu đãi thanh toán theo ngân hàng/ví (carousel ở PDP). SPEC §5.</summary>
public class PaymentPromotion : BaseAuditableEntity
{
    public string Bank { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
}
