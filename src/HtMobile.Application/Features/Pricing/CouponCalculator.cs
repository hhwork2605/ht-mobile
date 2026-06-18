using HtMobile.Domain.Entities.Pricing;
using HtMobile.Domain.Enums;

namespace HtMobile.Application.Features.Pricing;

/// <summary>Kết quả áp mã giảm giá.</summary>
public enum CouponOutcome
{
    Ok,
    NotFound,        // không có voucher khớp mã
    Inactive,        // hết hạn / chưa bắt đầu
    BelowMinOrder,   // chưa đạt đơn tối thiểu
    NotApplicable,   // loại KM không phải giảm tiền (Gift/Combo/BankOffer…)
}

/// <summary>Kết quả đánh giá voucher cho 1 subtotal.</summary>
public readonly record struct CouponResult(CouponOutcome Outcome, decimal Discount, decimal? MinOrder)
{
    public bool IsApplied => Outcome == CouponOutcome.Ok && Discount > 0;
}

/// <summary>
/// Tính giảm giá từ voucher (THUẦN, dễ test). Validate hạn + đơn tối thiểu (ConditionsJson.minOrder),
/// rồi tính số tiền giảm theo <see cref="PromotionType"/>:
/// Percentage = subtotal × Value%, FixedAmount = Value — đều bị chặn trong [0, subtotal].
/// </summary>
public static class CouponCalculator
{
    public static CouponResult Evaluate(Promotion? promo, decimal subtotal, DateTime now)
    {
        if (promo is null || string.IsNullOrWhiteSpace(promo.Code))
            return new CouponResult(CouponOutcome.NotFound, 0m, null);

        if (!promo.IsActiveAt(now))
            return new CouponResult(CouponOutcome.Inactive, 0m, null);

        var minOrder = PromotionConditions.GetMinOrder(promo.ConditionsJson);
        if (minOrder is > 0 && subtotal < minOrder)
            return new CouponResult(CouponOutcome.BelowMinOrder, 0m, minOrder);

        var discount = promo.Type switch
        {
            PromotionType.Percentage => Math.Round(subtotal * promo.Value / 100m, MidpointRounding.AwayFromZero),
            PromotionType.FixedAmount or PromotionType.Voucher => promo.Value,
            _ => -1m,   // loại không phải giảm tiền
        };

        if (discount < 0)
            return new CouponResult(CouponOutcome.NotApplicable, 0m, minOrder);

        // Chặn trong [0, subtotal] — không bao giờ âm tổng.
        discount = Math.Clamp(discount, 0m, subtotal);
        return new CouponResult(CouponOutcome.Ok, discount, minOrder);
    }
}
