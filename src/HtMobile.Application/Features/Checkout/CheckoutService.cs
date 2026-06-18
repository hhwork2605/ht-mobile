using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Cart;
using HtMobile.Application.Features.Checkout.Dtos;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Checkout;

/// <summary>
/// Đặt hàng từ giỏ (Phase 2, COD). Giá dòng lấy qua giỏ (đã định giá bằng <c>IPricingService</c>),
/// snapshot vào <c>OrderItem.UnitPrice</c>. Tính tiền/dựng đơn ở <see cref="OrderFactory"/> (thuần).
/// </summary>
public class CheckoutService
{
    public const string CodPaymentMethod = "COD";

    private readonly IApplicationDbContext _db;
    private readonly ICartService _cart;

    public CheckoutService(IApplicationDbContext db, ICartService cart)
    {
        _db = db;
        _cart = cart;
    }

    /// <summary>Tóm tắt giỏ cho trang checkout (đơn giá hiệu lực + tổng).</summary>
    public Task<Cart.Dtos.CartDto> GetSummaryAsync(CartOwner owner, CancellationToken ct = default)
        => _cart.GetCartAsync(owner, ct);

    /// <summary>Đặt hàng từ giỏ của owner. Trả null nếu giỏ rỗng (không tạo đơn).</summary>
    public async Task<PlaceOrderResult?> PlaceOrderAsync(
        CartOwner owner, string shippingAddress, CancellationToken ct = default)
    {
        var cart = await _cart.GetCartAsync(owner, ct);
        if (cart.IsEmpty) return null;

        var lines = cart.Items
            .Select(i => new OrderLineInput(i.VariantId, i.UnitPrice, i.Quantity))   // CartLineDto.VariantId = Id Product con
            .ToList();

        // cart.Discount/CouponCode đã được revalidate trong GetCartAsync (hạn + ngưỡng đơn) tại thời điểm đặt.
        var order = OrderFactory.Create(lines, owner.CustomerId, shippingAddress, CodPaymentMethod, cart.Discount, cart.CouponCode);
        if (order is null) return null;

        // Atomic: tạo đơn + xoá giỏ trong CÙNG 1 SaveChanges (tránh đơn tạo mà giỏ chưa xoá → đặt lại).
        _db.Orders.Add(order);
        var cartEntity = await FindOwnerCartAsync(owner, ct);
        if (cartEntity is not null) _db.Carts.Remove(cartEntity);
        await _db.SaveChangesAsync(ct);

        return new PlaceOrderResult(order.Id);
    }

    private Task<Domain.Entities.Sales.Cart?> FindOwnerCartAsync(CartOwner owner, CancellationToken ct)
    {
        if (owner.IsUser)
            return _db.Carts.FirstOrDefaultAsync(c => c.CustomerId == owner.CustomerId, ct);
        if (owner.SessionId is null)
            return Task.FromResult<Domain.Entities.Sales.Cart?>(null);
        return _db.Carts.FirstOrDefaultAsync(c => c.SessionId == owner.SessionId && c.CustomerId == null, ct);
    }
}
