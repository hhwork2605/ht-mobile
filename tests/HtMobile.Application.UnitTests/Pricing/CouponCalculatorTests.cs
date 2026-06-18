using FluentAssertions;
using HtMobile.Application.Features.Pricing;
using HtMobile.Domain.Entities.Pricing;
using HtMobile.Domain.Enums;
using Xunit;

namespace HtMobile.Application.UnitTests.Pricing;

public class CouponCalculatorTests
{
    private static readonly DateTime Now = new(2026, 6, 18, 0, 0, 0, DateTimeKind.Utc);

    private static Promotion Voucher(PromotionType type, decimal value, string? conditions = null) => new()
    {
        Name = "V", Code = "SALE", Type = type, Value = value,
        StartsAt = Now.AddDays(-1), EndsAt = Now.AddDays(1), ConditionsJson = conditions,
    };

    [Fact]
    public void Null_promo_is_not_found()
        => CouponCalculator.Evaluate(null, 1_000_000m, Now).Outcome.Should().Be(CouponOutcome.NotFound);

    [Fact]
    public void Promo_without_code_is_not_a_voucher()
    {
        var promo = Voucher(PromotionType.Percentage, 10);
        promo.Code = null;
        CouponCalculator.Evaluate(promo, 1_000_000m, Now).Outcome.Should().Be(CouponOutcome.NotFound);
    }

    [Fact]
    public void Expired_voucher_is_inactive()
    {
        var promo = Voucher(PromotionType.Percentage, 10);
        promo.EndsAt = Now.AddDays(-1);
        CouponCalculator.Evaluate(promo, 1_000_000m, Now).Outcome.Should().Be(CouponOutcome.Inactive);
    }

    [Fact]
    public void Percentage_discount_is_computed()
    {
        var r = CouponCalculator.Evaluate(Voucher(PromotionType.Percentage, 10), 2_000_000m, Now);
        r.Outcome.Should().Be(CouponOutcome.Ok);
        r.Discount.Should().Be(200_000m);
    }

    [Fact]
    public void Fixed_discount_is_capped_at_subtotal()
    {
        var r = CouponCalculator.Evaluate(Voucher(PromotionType.FixedAmount, 500_000), 300_000m, Now);
        r.Discount.Should().Be(300_000m);   // không vượt subtotal
    }

    [Fact]
    public void Below_min_order_is_rejected()
    {
        var promo = Voucher(PromotionType.FixedAmount, 500_000, "{\"minOrder\":10000000}");
        var r = CouponCalculator.Evaluate(promo, 5_000_000m, Now);
        r.Outcome.Should().Be(CouponOutcome.BelowMinOrder);
        r.Discount.Should().Be(0m);
    }

    [Fact]
    public void At_or_above_min_order_applies()
    {
        var promo = Voucher(PromotionType.FixedAmount, 500_000, "{\"minOrder\":10000000}");
        var r = CouponCalculator.Evaluate(promo, 10_000_000m, Now);
        r.Outcome.Should().Be(CouponOutcome.Ok);
        r.Discount.Should().Be(500_000m);
    }

    [Fact]
    public void Non_discount_type_is_not_applicable()
        => CouponCalculator.Evaluate(Voucher(PromotionType.Gift, 0), 1_000_000m, Now)
            .Outcome.Should().Be(CouponOutcome.NotApplicable);
}
