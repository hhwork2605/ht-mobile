using HtMobile.Application.Features.Cart;
using HtMobile.Application.Features.Cart.Dtos;

namespace HtMobile.Application.Common.Interfaces;

/// <summary>Hợp đồng thao tác giỏ hàng (Phase 2). Hiện thực: <c>Features.Cart.CartService</c>.</summary>
public interface ICartService
{
    /// <summary>Đọc giỏ + tóm tắt tiền (đơn giá qua IPricingService). Giỏ chưa có → trả giỏ rỗng.</summary>
    Task<CartDto> GetCartAsync(CartOwner owner, CancellationToken ct = default);

    /// <summary>Thêm biến thể (gộp nếu đã có). <paramref name="unitPriceOverride"/> ≠ null = giá cố định (vd mua kèm bundle). Trả tổng số lượng giỏ.</summary>
    Task<int> AddItemAsync(CartOwner owner, long variantId, int quantity = 1, decimal? unitPriceOverride = null, CancellationToken ct = default);

    /// <summary>Tăng/giảm số lượng 1 dòng (≤0 thì xoá). Trả tổng số lượng giỏ.</summary>
    Task<int> UpdateQuantityAsync(CartOwner owner, long cartItemId, int delta, CancellationToken ct = default);

    /// <summary>Xoá 1 dòng. Trả tổng số lượng giỏ.</summary>
    Task<int> RemoveItemAsync(CartOwner owner, long cartItemId, CancellationToken ct = default);

    /// <summary>Tổng số lượng (badge header).</summary>
    Task<int> GetCountAsync(CartOwner owner, CancellationToken ct = default);

    /// <summary>Gộp giỏ khách vãng lai vào giỏ user khi đăng nhập.</summary>
    Task MergeAsync(string sessionId, long customerId, CancellationToken ct = default);

    /// <summary>Áp mã giảm giá vào giỏ (validate code + hạn + đơn tối thiểu). Trả kết quả + giỏ sau khi áp.</summary>
    Task<ApplyCouponResult> ApplyCouponAsync(CartOwner owner, string code, CancellationToken ct = default);

    /// <summary>Gỡ mã giảm giá khỏi giỏ.</summary>
    Task RemoveCouponAsync(CartOwner owner, CancellationToken ct = default);
}
