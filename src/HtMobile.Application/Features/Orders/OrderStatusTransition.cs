using HtMobile.Domain.Enums;

namespace HtMobile.Application.Features.Orders;

/// <summary>Luồng chuyển trạng thái đơn hợp lệ (THUẦN, dễ test). Admin chỉ được đổi theo các bước cho phép.</summary>
public static class OrderStatusTransition
{
    private static readonly IReadOnlyDictionary<OrderStatus, OrderStatus[]> Map = new Dictionary<OrderStatus, OrderStatus[]>
    {
        [OrderStatus.Pending] = new[] { OrderStatus.Confirmed, OrderStatus.Cancelled },
        [OrderStatus.Confirmed] = new[] { OrderStatus.Processing, OrderStatus.Cancelled },
        [OrderStatus.Processing] = new[] { OrderStatus.Shipped, OrderStatus.Cancelled },
        [OrderStatus.Shipped] = new[] { OrderStatus.Delivered },
        [OrderStatus.Delivered] = new[] { OrderStatus.Refunded },
        [OrderStatus.Cancelled] = Array.Empty<OrderStatus>(),
        [OrderStatus.Refunded] = Array.Empty<OrderStatus>(),
    };

    /// <summary>Các trạng thái có thể chuyển tới từ <paramref name="from"/>.</summary>
    public static IReadOnlyList<OrderStatus> AllowedNext(OrderStatus from)
        => Map.TryGetValue(from, out var next) ? next : Array.Empty<OrderStatus>();

    /// <summary>Có được phép chuyển <paramref name="from"/> → <paramref name="to"/> không.</summary>
    public static bool CanTransition(OrderStatus from, OrderStatus to)
        => AllowedNext(from).Contains(to);
}
