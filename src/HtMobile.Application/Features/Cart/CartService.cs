using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Cart.Dtos;
using HtMobile.Application.Features.Pricing;
using HtMobile.Domain.Entities.Sales;
using HtMobile.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Cart;

/// <summary>Chủ sở hữu giỏ: user đăng nhập (CustomerId) hoặc khách vãng lai (SessionId = cookie GUID).</summary>
public record CartOwner(long? CustomerId, string? SessionId)
{
    public bool IsUser => CustomerId is not null;
}

/// <summary>
/// Thao tác giỏ hàng (Phase 2). Đơn giá hiển thị luôn lấy qua <see cref="IPricingService"/> (giá hiệu lực
/// hiện tại) — <see cref="CartItem.UnitPrice"/> chỉ là snapshot lúc thêm. Tính tiền thuần ở <see cref="CartMath"/>.
/// </summary>
public class CartService : ICartService
{
    private readonly IApplicationDbContext _db;
    private readonly IPricingService _pricing;
    private readonly IDateTime _clock;

    public CartService(IApplicationDbContext db, IPricingService pricing, IDateTime clock)
    {
        _db = db;
        _pricing = pricing;
        _clock = clock;
    }

    /// <summary>Đọc giỏ + tính tóm tắt. Không tạo giỏ nếu chưa có (trả giỏ rỗng).</summary>
    public async Task<CartDto> GetCartAsync(CartOwner owner, CancellationToken ct = default)
    {
        var cart = await FindCartAsync(owner, track: false, ct);
        if (cart is null || cart.Items.Count == 0)
            return new CartDto();

        var lines = new List<CartLineDto>();
        var amounts = new List<CartLineAmount>();

        foreach (var item in cart.Items.OrderBy(i => i.Id))
        {
            // Giá cố định (mua kèm bundle) thì dùng đúng; ngược lại lấy giá hiệu lực qua IPricingService.
            var unitPrice = item.UnitPriceOverride
                ?? (await _pricing.GetEffectivePriceAsync(item.ProductId, ct)).FinalPrice;
            var variant = item.Product;            // biến thể = Product con
            var model = variant.Parent ?? variant; // model cha (tên + ảnh gallery)
            var variantText = string.Join(" · ", variant.Attributes
                .OrderBy(a => a.Attribute.SortOrder).ThenBy(a => a.AttributeId).Select(a => a.Value));
            var thumb = model.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault();

            lines.Add(new CartLineDto
            {
                Id = item.Id,
                VariantId = item.ProductId,
                ProductName = model.Name,
                VariantText = variantText,
                ThumbnailUrl = thumb,
                VariantSlug = variant.Slug,
                UnitPrice = unitPrice,
                Quantity = item.Quantity,
                LineTotal = unitPrice * item.Quantity
            });
            amounts.Add(new CartLineAmount(unitPrice, item.Quantity));
        }

        var totals = CartMath.Summarize(amounts);
        var (couponCode, discount) = await EvaluateCouponAsync(cart.CouponCode, totals.Subtotal, ct);
        return new CartDto
        {
            Items = lines,
            Count = totals.Count,
            Subtotal = totals.Subtotal,
            ShippingFee = totals.ShippingFee,
            CouponCode = couponCode,
            Discount = discount,
            Total = totals.Total - discount
        };
    }

    /// <summary>Tra voucher theo mã đã lưu + đánh giá với subtotal hiện tại. Trả (mã hiển thị, số tiền giảm).
    /// Mã không còn hợp lệ (hết hạn / dưới ngưỡng) → (null, 0) — vẫn giữ mã trên entity để tự áp lại khi đủ điều kiện.</summary>
    private async Task<(string? Code, decimal Discount)> EvaluateCouponAsync(string? storedCode, decimal subtotal, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(storedCode)) return (null, 0m);
        var promo = await _db.Promotions.AsNoTracking().FirstOrDefaultAsync(p => p.Code == storedCode, ct);
        var r = CouponCalculator.Evaluate(promo, subtotal, _clock.Now);
        return r.IsApplied ? (storedCode, r.Discount) : (null, 0m);
    }

    public async Task<ApplyCouponResult> ApplyCouponAsync(CartOwner owner, string code, CancellationToken ct = default)
    {
        var normalized = (code ?? string.Empty).Trim().ToUpperInvariant();
        var cart = await FindCartAsync(owner, track: true, ct);
        if (cart is null || cart.Items.Count == 0)
            return new ApplyCouponResult(CouponOutcome.NotFound, await GetCartAsync(owner, ct));

        // Subtotal hiện tại để xét ngưỡng đơn tối thiểu.
        var summary = await GetCartAsync(owner, ct);
        var promo = string.IsNullOrEmpty(normalized)
            ? null
            : await _db.Promotions.AsNoTracking().FirstOrDefaultAsync(p => p.Code == normalized, ct);
        var result = CouponCalculator.Evaluate(promo, summary.Subtotal, _clock.Now);

        if (result.Outcome == CouponOutcome.Ok)
        {
            cart.CouponCode = normalized;
            await _db.SaveChangesAsync(ct);
        }

        return new ApplyCouponResult(result.Outcome, await GetCartAsync(owner, ct));
    }

    public async Task RemoveCouponAsync(CartOwner owner, CancellationToken ct = default)
    {
        var cart = await FindCartAsync(owner, track: true, ct);
        if (cart?.CouponCode is null) return;
        cart.CouponCode = null;
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Thêm 1 biến thể vào giỏ (gộp nếu đã có). Trả về tổng số lượng giỏ sau khi thêm.</summary>
    public async Task<int> AddItemAsync(CartOwner owner, long variantId, int quantity = 1, decimal? unitPriceOverride = null, CancellationToken ct = default)
    {
        if (quantity <= 0) quantity = 1;

        // Chỉ thêm biến thể (Product con — ProductParentId != null) có thật + đang bán. Model cha không bán trực tiếp.
        var sellable = await _db.Products
            .AsNoTracking()
            .AnyAsync(p => p.Id == variantId && p.ProductParentId != null && p.Status == ProductStatus.Active, ct);
        if (!sellable) return await GetCountAsync(owner, ct);

        var cart = await FindCartAsync(owner, track: true, ct) ?? await CreateCartAsync(owner, ct);

        var existing = cart.Items.FirstOrDefault(i => i.ProductId == variantId);
        if (existing is not null)
        {
            existing.Quantity += quantity;
            // Mua kèm bundle: áp giá ưu đãi cho CẢ dòng (do unique (CartId,VariantId) gộp 1 dòng).
            // Có chủ đích Phase 3: có lợi cho khách, không bao giờ tính cao hơn giá thường. Việc tách
            // giá theo từng đơn vị / áp giá kèm khi thêm lẻ là out-of-scope (xem P3-01).
            if (unitPriceOverride is not null)
            {
                existing.UnitPriceOverride = unitPriceOverride;
                existing.UnitPrice = unitPriceOverride.Value;
            }
        }
        else
        {
            var unit = unitPriceOverride ?? (await _pricing.GetEffectivePriceAsync(variantId, ct)).FinalPrice;
            cart.Items.Add(new CartItem
            {
                ProductId = variantId,
                Quantity = quantity,
                UnitPrice = unit,                       // snapshot lúc thêm
                UnitPriceOverride = unitPriceOverride   // null = theo giá hiệu lực
            });
        }

        await _db.SaveChangesAsync(ct);
        return cart.Items.Sum(i => i.Quantity);
    }

    /// <summary>Tăng/giảm số lượng 1 dòng (về ≤0 thì xoá). Chỉ tác động dòng thuộc giỏ của owner.</summary>
    public async Task<int> UpdateQuantityAsync(CartOwner owner, long cartItemId, int delta, CancellationToken ct = default)
    {
        var cart = await FindCartAsync(owner, track: true, ct);
        var item = cart?.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (cart is null || item is null) return cart?.Items.Sum(i => i.Quantity) ?? 0;

        var next = CartMath.NextQuantity(item.Quantity, delta);
        if (next <= 0)
            cart.Items.Remove(item);
        else
            item.Quantity = next;

        await _db.SaveChangesAsync(ct);
        return cart.Items.Sum(i => i.Quantity);
    }

    /// <summary>Xoá 1 dòng khỏi giỏ (chỉ khi thuộc giỏ của owner).</summary>
    public async Task<int> RemoveItemAsync(CartOwner owner, long cartItemId, CancellationToken ct = default)
    {
        var cart = await FindCartAsync(owner, track: true, ct);
        var item = cart?.Items.FirstOrDefault(i => i.Id == cartItemId);
        if (cart is null || item is null) return cart?.Items.Sum(i => i.Quantity) ?? 0;

        cart.Items.Remove(item);
        await _db.SaveChangesAsync(ct);
        return cart.Items.Sum(i => i.Quantity);
    }

    /// <summary>Tổng số lượng trong giỏ (cho badge header).</summary>
    public async Task<int> GetCountAsync(CartOwner owner, CancellationToken ct = default)
    {
        var cartId = await FindCartIdAsync(owner, ct);
        if (cartId is null) return 0;
        return await _db.CartItems.Where(i => i.CartId == cartId).SumAsync(i => (int?)i.Quantity, ct) ?? 0;
    }

    /// <summary>Gộp giỏ khách vãng lai vào giỏ user khi đăng nhập, rồi xoá giỏ vãng lai.</summary>
    public async Task MergeAsync(string sessionId, long customerId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId)) return;

        var guest = await _db.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.CustomerId == null, ct);
        if (guest is null || guest.Items.Count == 0)
        {
            if (guest is not null) _db.Carts.Remove(guest);
            await _db.SaveChangesAsync(ct);
            return;
        }

        var user = await _db.Carts.Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);
        if (user is null)
        {
            user = new Domain.Entities.Sales.Cart { CustomerId = customerId };
            _db.Carts.Add(user);
        }

        var merged = CartMath.Merge(
            user.Items.Select(i => (i.ProductId, i.Quantity)),
            guest.Items.Select(i => (i.ProductId, i.Quantity)));

        // Giá snapshot cho dòng mới: ưu tiên dòng giỏ user, sau đó dòng guest (tránh UnitPrice=0).
        var unitPriceByVariant = user.Items.Concat(guest.Items)
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.First().UnitPrice);

        // Chỉ giữ biến thể (Product con) còn bán khi gộp.
        var variantIds = merged.Select(m => m.VariantId).ToList();
        var activeIds = (await _db.Products
                .Where(p => variantIds.Contains(p.Id) && p.Status == ProductStatus.Active)
                .Select(p => p.Id)
                .ToListAsync(ct))
            .ToHashSet();

        foreach (var (variantId, qty) in merged)
        {
            if (!activeIds.Contains(variantId)) continue;
            var line = user.Items.FirstOrDefault(i => i.ProductId == variantId);
            if (line is not null)
                line.Quantity = qty;
            else
                user.Items.Add(new CartItem
                {
                    ProductId = variantId,
                    Quantity = qty,
                    UnitPrice = unitPriceByVariant.GetValueOrDefault(variantId)
                });
        }

        _db.Carts.Remove(guest);
        await _db.SaveChangesAsync(ct);
    }

    // ----- helpers -----

    private async Task<Domain.Entities.Sales.Cart?> FindCartAsync(CartOwner owner, bool track, CancellationToken ct)
    {
        IQueryable<Domain.Entities.Sales.Cart> q = _db.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Attributes).ThenInclude(a => a.Attribute)
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Parent).ThenInclude(pp => pp!.Images);
        if (!track) q = q.AsNoTracking();

        return owner.IsUser
            ? await q.FirstOrDefaultAsync(c => c.CustomerId == owner.CustomerId, ct)
            : owner.SessionId is null ? null
              : await q.FirstOrDefaultAsync(c => c.SessionId == owner.SessionId && c.CustomerId == null, ct);
    }

    private async Task<long?> FindCartIdAsync(CartOwner owner, CancellationToken ct)
    {
        if (owner.IsUser)
            return await _db.Carts.Where(c => c.CustomerId == owner.CustomerId)
                .Select(c => (long?)c.Id).FirstOrDefaultAsync(ct);
        if (owner.SessionId is null) return null;
        return await _db.Carts.Where(c => c.SessionId == owner.SessionId && c.CustomerId == null)
            .Select(c => (long?)c.Id).FirstOrDefaultAsync(ct);
    }

    private async Task<Domain.Entities.Sales.Cart> CreateCartAsync(CartOwner owner, CancellationToken ct)
    {
        var cart = new Domain.Entities.Sales.Cart
        {
            CustomerId = owner.CustomerId,
            SessionId = owner.IsUser ? null : owner.SessionId
        };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync(ct);
        return cart;
    }
}
