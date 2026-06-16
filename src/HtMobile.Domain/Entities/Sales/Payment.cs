using HtMobile.Domain.Common;
using HtMobile.Domain.Enums;

namespace HtMobile.Domain.Entities.Sales;

/// <summary>Giao dịch thanh toán — uỷ thác cổng PCI-DSS, KHÔNG lưu dữ liệu thẻ. SPEC §13.</summary>
public class Payment : BaseAuditableEntity
{
    public long OrderId { get; set; }
    public string? Provider { get; set; }   // VNPAY / ZaloPay…
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public decimal Amount { get; set; }
    public string? TxnRef { get; set; }

    public Order Order { get; set; } = null!;
}
