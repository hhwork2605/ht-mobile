using HtMobile.Domain.Common;
using HtMobile.Domain.Enums;

namespace HtMobile.Domain.Entities.Services;

/// <summary>Yêu cầu thu cũ đổi mới (định giá máy cũ + trợ giá). SPEC §4.2, UC-04.</summary>
public class TradeInRequest : BaseAuditableEntity
{
    public long? CustomerId { get; set; }
    public string Model { get; set; } = string.Empty;
    public string? Condition { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public TradeInStatus Status { get; set; } = TradeInStatus.Pending;
}
