namespace HtMobile.Application.Features.Cart;

/// <summary>Một dòng giỏ để tính tiền (đơn giá hiệu lực × số lượng).</summary>
public readonly record struct CartLineAmount(decimal UnitPrice, int Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}

/// <summary>Tóm tắt tiền của giỏ.</summary>
public readonly record struct CartTotals(int Count, decimal Subtotal, decimal ShippingFee, decimal Total);

/// <summary>
/// Logic tính toán giỏ THUẦN (không phụ thuộc DB/cache) để dễ unit-test — tương tự <c>PriceCalculator</c>.
/// </summary>
public static class CartMath
{
    /// <summary>Phí vận chuyển Phase 2: miễn phí.</summary>
    public const decimal ShippingFee = 0m;

    /// <summary>Tổng số lượng + tạm tính (Σ đơn giá×qty) + phí ship + tổng cộng.</summary>
    public static CartTotals Summarize(IEnumerable<CartLineAmount> lines)
    {
        var count = 0;
        var subtotal = 0m;
        foreach (var line in lines)
        {
            count += line.Quantity;
            subtotal += line.LineTotal;
        }
        return new CartTotals(count, subtotal, ShippingFee, subtotal + ShippingFee);
    }

    /// <summary>Số lượng mới sau khi +/- delta; không bao giờ âm (≤0 nghĩa là sẽ bị xoá).</summary>
    public static int NextQuantity(int current, int delta)
    {
        var next = current + delta;
        return next < 0 ? 0 : next;
    }

    /// <summary>Gộp giỏ: cộng dồn số lượng theo VariantId (dùng khi merge giỏ guest → giỏ user).</summary>
    public static IReadOnlyList<(long VariantId, int Quantity)> Merge(
        IEnumerable<(long VariantId, int Quantity)> existing,
        IEnumerable<(long VariantId, int Quantity)> incoming)
    {
        var byVariant = new Dictionary<long, int>();
        var order = new List<long>();
        foreach (var (variantId, qty) in existing.Concat(incoming))
        {
            if (byVariant.TryGetValue(variantId, out var cur))
                byVariant[variantId] = cur + qty;
            else
            {
                byVariant[variantId] = qty;
                order.Add(variantId);
            }
        }
        return order.Select(v => (v, byVariant[v])).ToList();
    }
}
