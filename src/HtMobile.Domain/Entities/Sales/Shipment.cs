using HtMobile.Domain.Common;
using HtMobile.Domain.Enums;

namespace HtMobile.Domain.Entities.Sales;

public class Shipment : BaseAuditableEntity
{
    public long OrderId { get; set; }
    public string? Address { get; set; }
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;
    public string? TrackingNo { get; set; }

    public Order Order { get; set; } = null!;
}
