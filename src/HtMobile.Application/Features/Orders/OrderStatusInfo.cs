using HtMobile.Domain.Enums;

namespace HtMobile.Application.Features.Orders;

/// <summary>Mô tả trạng thái đơn cho UI: nhãn + chỉ số bước (0..3) trên thanh tiến trình; -1 nếu huỷ/hoàn tiền.</summary>
public readonly record struct OrderStatusView(string Label, int StepIndex, bool IsCancelled);

/// <summary>Map <see cref="OrderStatus"/> → hiển thị (THUẦN, dễ test). 4 bước theo thiết kế ShopDunk.</summary>
public static class OrderStatusInfo
{
    /// <summary>Nhãn 4 bước trên thanh tiến trình.</summary>
    public static readonly string[] Steps = { "Đã đặt", "Đang xử lý", "Đang giao", "Hoàn thành" };

    public static OrderStatusView Describe(OrderStatus status) => status switch
    {
        OrderStatus.Pending => new("Chờ xác nhận", 0, false),
        OrderStatus.Confirmed => new("Đã xác nhận", 1, false),
        OrderStatus.Processing => new("Đang xử lý", 1, false),
        OrderStatus.Shipped => new("Đang giao", 2, false),
        OrderStatus.Delivered => new("Hoàn thành", 3, false),
        OrderStatus.Cancelled => new("Đã huỷ", -1, true),
        OrderStatus.Refunded => new("Đã hoàn tiền", -1, true),
        _ => new(status.ToString(), 0, false)
    };
}
