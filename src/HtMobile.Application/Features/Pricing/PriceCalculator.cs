using HtMobile.Application.Features.Pricing.Dtos;
using HtMobile.Domain.Entities.Pricing;
using HtMobile.Domain.Enums;

namespace HtMobile.Application.Features.Pricing;

/// <summary>
/// Logic tính giá THUẦN (không phụ thuộc DB/cache) để dễ unit-test.
/// Quy tắc scaffold: chọn khuyến mãi giảm-giá (Percentage/FixedAmount) đang hiệu lực cho ra giá thấp nhất.
/// Bổ sung điều kiện theo <c>ConditionsJson</c> ở các phase sau.
/// </summary>
public static class PriceCalculator
{
    public static EffectivePrice Calculate(
        long variantId,
        decimal listPrice,
        decimal? compareAtPrice,
        IEnumerable<Promotion> promotions,
        DateTime now)
    {
        decimal finalPrice = listPrice;
        string? appliedName = null;

        foreach (var promo in promotions.Where(p => p.IsActiveAt(now)))
        {
            decimal candidate = promo.Type switch
            {
                PromotionType.Percentage => listPrice * (1 - Clamp01(promo.Value / 100m)),
                PromotionType.FixedAmount => listPrice - promo.Value,
                _ => listPrice // Gift/Voucher/Combo/BankOffer: không đổi giá niêm yết ở bước này
            };

            if (candidate < finalPrice)
            {
                finalPrice = candidate;
                appliedName = promo.Name;
            }
        }

        if (finalPrice < 0) finalPrice = 0;

        decimal anchor = compareAtPrice is > 0 ? compareAtPrice.Value : listPrice;
        int discountPercent = anchor > 0
            ? (int)Math.Round((anchor - finalPrice) / anchor * 100m, MidpointRounding.AwayFromZero)
            : 0;
        if (discountPercent < 0) discountPercent = 0;

        return new EffectivePrice
        {
            VariantId = variantId,
            ListPrice = listPrice,
            CompareAtPrice = compareAtPrice,
            FinalPrice = finalPrice,
            DiscountPercent = discountPercent,
            AppliedPromotionName = appliedName
        };
    }

    private static decimal Clamp01(decimal v) => v < 0 ? 0 : v > 1 ? 1 : v;
}
