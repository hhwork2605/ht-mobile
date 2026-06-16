using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Cart.Dtos;
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

    public CartService(IApplicationDbContext db, IPricingService pricing)
    {
        _db = db;
        _pricing = pricing;
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
            var price = await _pricing.GetEffectivePriceAsync(item.VariantId, ct);
            var variant = item.Variant;
            var product = variant.Product;
            var variantText = string.Join(" · ",
                new[] { variant.Color, variant.Storage }.Where(s => !string.IsNullOrWhiteSpace(s)));
            var thumb = product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault();

            lines.Add(new CartLineDto
            {
                Id = item.Id,
                VariantId = item.VariantId,
                ProductName = product.Name,
                VariantText = variantText,
                ThumbnailUrl = thumb,
                VariantSlug = variant.Slug,
                UnitPrice = price.FinalPrice,
                Quantity = item.Quantity,
                LineTotal = price.FinalPrice * item.Quantity
            });
            amounts.Add(new CartLineAmount(price.FinalPrice, item.Quantity));
        }

        var totals = CartMath.Summarize(amounts);
        return new CartDto
        {
            Items = lines,
            Count = totals.Count,
            Subtotal = totals.Subtotal,
            ShippingFee = totals.ShippingFee,
            Total = totals.Total
        };
    }

    /// <summary>Thêm 1 biến thể vào giỏ (gộp nếu đã có). Trả về tổng số lượng giỏ sau khi thêm.</summary>
    public async Task<int> AddItemAsync(CartOwner owner, long variantId, int quantity = 1, CancellationToken ct = default)
    {
        if (quantity <= 0) quantity = 1;

        // Chỉ thêm biến thể có thật + đang bán.
        var sellable = await _db.ProductVariants
            .AsNoTracking()
            .AnyAsync(v => v.Id == variantId && v.Status == VariantStatus.Active, ct);
        if (!sellable) return await GetCountAsync(owner, ct);

        var cart = await FindCartAsync(owner, track: true, ct) ?? await CreateCartAsync(owner, ct);

        var existing = cart.Items.FirstOrDefault(i => i.VariantId == variantId);
        if (existing is not null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            var price = await _pricing.GetEffectivePriceAsync(variantId, ct);
            cart.Items.Add(new CartItem
            {
                VariantId = variantId,
                Quantity = quantity,
                UnitPrice = price.FinalPrice   // snapshot lúc thêm
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
            user.Items.Select(i => (i.VariantId, i.Quantity)),
            guest.Items.Select(i => (i.VariantId, i.Quantity)));

        // Giá snapshot cho dòng mới: ưu tiên dòng giỏ user, sau đó dòng guest (tránh UnitPrice=0).
        var unitPriceByVariant = user.Items.Concat(guest.Items)
            .GroupBy(i => i.VariantId)
            .ToDictionary(g => g.Key, g => g.First().UnitPrice);

        // Chỉ giữ biến thể còn bán khi gộp.
        var variantIds = merged.Select(m => m.VariantId).ToList();
        var activeIds = (await _db.ProductVariants
                .Where(v => variantIds.Contains(v.Id) && v.Status == VariantStatus.Active)
                .Select(v => v.Id)
                .ToListAsync(ct))
            .ToHashSet();

        foreach (var (variantId, qty) in merged)
        {
            if (!activeIds.Contains(variantId)) continue;
            var line = user.Items.FirstOrDefault(i => i.VariantId == variantId);
            if (line is not null)
                line.Quantity = qty;
            else
                user.Items.Add(new CartItem
                {
                    VariantId = variantId,
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
            .Include(c => c.Items).ThenInclude(i => i.Variant).ThenInclude(v => v.Product).ThenInclude(p => p.Images);
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
