namespace HtMobile.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    Refunded = 6
}

public enum PaymentStatus
{
    Pending = 0,
    Authorized = 1,
    Paid = 2,
    Failed = 3,
    Refunded = 4
}

public enum ShipmentStatus
{
    Pending = 0,
    Preparing = 1,
    Shipped = 2,
    Delivered = 3,
    Returned = 4
}

/// <summary>Loại khuyến mãi nhiều tầng (SPEC §4.2).</summary>
public enum PromotionType
{
    Percentage = 0,   // giảm %
    FixedAmount = 1,  // giảm số tiền
    Gift = 2,         // quà tặng
    Voucher = 3,      // voucher
    Combo = 4,        // combo
    BankOffer = 5     // ưu đãi theo ngân hàng/ví
}

/// <summary>Trạng thái 1 sản phẩm/biến thể (biến thể = Product con). Trước đây là VariantStatus.</summary>
public enum ProductStatus
{
    Active = 0,
    OutOfStock = 1,
    Discontinued = 2
}

public enum TradeInStatus
{
    Pending = 0,
    Quoted = 1,
    Accepted = 2,
    Rejected = 3,
    Completed = 4
}
