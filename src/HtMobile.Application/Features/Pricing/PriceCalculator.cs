using HtMobile.Application.Features.Pricing.Dtos;
using HtMobile.Domain.Entities.Pricing;
using HtMobile.Domain.Enums;

namespace HtMobile.Application.Features.Pricing;

/// <summary>
/// Logic tính giá THUẦN (không phụ thuộc DB/cache) để dễ unit-test. Khuyến mãi đã được lọc theo điều kiện
/// (xem <see cref="PromotionConditions"/>) trước khi truyền vào. Áp <b>nhiều tầng</b>: BEST Percentage →
/// rồi BEST FixedAmount trên giá đã giảm (tối đa 1 mỗi loại). Tiền làm tròn về <b>đồng</b> (VND, không có hào).
/// </summary>
public static class PriceCalculator
{
    public static EffectivePrice Calculate(
        long productId,
        decimal listPrice,
        decimal? compareAtPrice,
        IEnumerable<Promotion> promotions,
        DateTime now)
    {
        // Nhiều tầng: áp BEST Percentage (giảm % lớn nhất) → rồi BEST FixedAmount trên giá đã giảm.
        // Gift/Voucher/Combo/BankOffer: không đổi giá (chỉ là ưu đãi liệt kê ở OfferList).
        var active = promotions.Where(p => p.IsActiveAt(now)).ToList();
        decimal finalPrice = listPrice;
        var appliedNames = new List<string>();

        // Tie-break theo Id để chọn KM tất định khi trùng Value (tên áp ổn định giữa các lần/cache).
        var bestPct = active
            .Where(p => p.Type == PromotionType.Percentage && p.Value > 0)
            .OrderByDescending(p => p.Value).ThenBy(p => p.Id)
            .FirstOrDefault();
        if (bestPct is not null)
        {
            finalPrice *= 1 - Clamp01(bestPct.Value / 100m);
            appliedNames.Add(bestPct.Name);
        }

        var bestFixed = active
            .Where(p => p.Type == PromotionType.FixedAmount && p.Value > 0)
            .OrderByDescending(p => p.Value).ThenBy(p => p.Id)
            .FirstOrDefault();
        if (bestFixed is not null)
        {
            finalPrice -= bestFixed.Value;
            appliedNames.Add(bestFixed.Name);
        }

        if (finalPrice < 0) finalPrice = 0;
        // Làm tròn về đồng (VND không có hào) — giữ PDP và tổng giỏ/đơn nhất quán.
        finalPrice = Math.Round(finalPrice, 0, MidpointRounding.AwayFromZero);
        string? appliedName = appliedNames.Count > 0 ? string.Join(" + ", appliedNames) : null;

        // Neo % giảm theo mốc cao nhất; bỏ qua compareAt nếu nhập sai (< listPrice).
        decimal anchor = compareAtPrice is { } ca && ca >= listPrice ? ca : listPrice;
        int discountPercent = anchor > 0
            ? (int)Math.Round((anchor - finalPrice) / anchor * 100m, MidpointRounding.AwayFromZero)
            : 0;
        if (discountPercent < 0) discountPercent = 0;

        return new EffectivePrice
        {
            ProductId = productId,
            ListPrice = listPrice,
            CompareAtPrice = compareAtPrice,
            FinalPrice = finalPrice,
            DiscountPercent = discountPercent,
            AppliedPromotionName = appliedName
        };
    }

    private static decimal Clamp01(decimal v) => v < 0 ? 0 : v > 1 ? 1 : v;
}
