using FluentAssertions;
using HtMobile.Application.Features.Pricing;
using HtMobile.Domain.Entities.Pricing;
using HtMobile.Domain.Enums;
using Xunit;

namespace HtMobile.Application.UnitTests.Pricing;

public class PriceCalculatorTests
{
    private static readonly DateTime Now = new(2026, 6, 16, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void No_promotion_returns_list_price()
    {
        var result = PriceCalculator.Calculate(1, listPrice: 1_000_000m, compareAtPrice: null,
            promotions: Array.Empty<Promotion>(), now: Now);

        result.FinalPrice.Should().Be(1_000_000m);
        result.DiscountPercent.Should().Be(0);
        result.AppliedPromotionName.Should().BeNull();
    }

    [Fact]
    public void Active_percentage_promotion_is_applied()
    {
        var promo = new Promotion
        {
            Name = "Sale 10%",
            Type = PromotionType.Percentage,
            Value = 10,
            StartsAt = Now.AddDays(-1),
            EndsAt = Now.AddDays(1)
        };

        var result = PriceCalculator.Calculate(1, listPrice: 1_000_000m, compareAtPrice: 1_200_000m,
            promotions: new[] { promo }, now: Now);

        result.FinalPrice.Should().Be(900_000m);
        // % giảm so với mốc cao nhất (compareAt = 1.2tr): (1.2tr - 0.9tr)/1.2tr = 25%
        result.DiscountPercent.Should().Be(25);
        result.AppliedPromotionName.Should().Be("Sale 10%");
    }

    [Fact]
    public void Expired_promotion_is_ignored()
    {
        var promo = new Promotion
        {
            Name = "Expired",
            Type = PromotionType.Percentage,
            Value = 50,
            StartsAt = Now.AddDays(-10),
            EndsAt = Now.AddDays(-5)
        };

        var result = PriceCalculator.Calculate(1, 1_000_000m, null, new[] { promo }, Now);

        result.FinalPrice.Should().Be(1_000_000m);
    }

    [Fact]
    public void Percentage_then_fixed_stack()
    {
        var p10 = new Promotion { Name = "10%", Type = PromotionType.Percentage, Value = 10, StartsAt = Now.AddDays(-1), EndsAt = Now.AddDays(1) };
        var pFixed = new Promotion { Name = "-300k", Type = PromotionType.FixedAmount, Value = 300_000m, StartsAt = Now.AddDays(-1), EndsAt = Now.AddDays(1) };

        var result = PriceCalculator.Calculate(1, 1_000_000m, null, new[] { p10, pFixed }, Now);

        // best % trước: 1.000.000 * 0.9 = 900.000; rồi best fixed: 900.000 - 300.000 = 600.000
        result.FinalPrice.Should().Be(600_000m);
        result.AppliedPromotionName.Should().Contain("10%").And.Contain("-300k");
    }

    [Fact]
    public void Only_best_percentage_among_many_percentages()
    {
        var p10 = new Promotion { Name = "10%", Type = PromotionType.Percentage, Value = 10, StartsAt = Now.AddDays(-1), EndsAt = Now.AddDays(1) };
        var p20 = new Promotion { Name = "20%", Type = PromotionType.Percentage, Value = 20, StartsAt = Now.AddDays(-1), EndsAt = Now.AddDays(1) };

        var result = PriceCalculator.Calculate(1, 1_000_000m, null, new[] { p10, p20 }, Now);

        result.FinalPrice.Should().Be(800_000m);   // chỉ best % (20%), không cộng dồn 2 %
        result.AppliedPromotionName.Should().Be("20%");
    }

    [Fact]
    public void Fixed_is_clamped_to_zero()
    {
        var pBig = new Promotion { Name = "-2tr", Type = PromotionType.FixedAmount, Value = 2_000_000m, StartsAt = Now.AddDays(-1), EndsAt = Now.AddDays(1) };

        var result = PriceCalculator.Calculate(1, 1_000_000m, null, new[] { pBig }, Now);

        result.FinalPrice.Should().Be(0m);
    }

    [Fact]
    public void Percentage_over_100_clamps_to_zero()
    {
        var p = new Promotion { Name = "150%", Type = PromotionType.Percentage, Value = 150, StartsAt = Now.AddDays(-1), EndsAt = Now.AddDays(1) };

        var result = PriceCalculator.Calculate(1, 1_000_000m, null, new[] { p }, Now);

        result.FinalPrice.Should().Be(0m);
    }

    [Fact]
    public void Final_price_is_rounded_to_whole_dong()
    {
        var p = new Promotion { Name = "10%", Type = PromotionType.Percentage, Value = 10, StartsAt = Now.AddDays(-1), EndsAt = Now.AddDays(1) };

        // 999.999 * 0.9 = 899.999,1 → làm tròn về đồng = 899.999
        var result = PriceCalculator.Calculate(1, 999_999m, null, new[] { p }, Now);

        result.FinalPrice.Should().Be(899_999m);
        (result.FinalPrice % 1m).Should().Be(0m);
    }

    [Fact]
    public void Gift_and_bankoffer_do_not_change_price()
    {
        var gift = new Promotion { Name = "Quà", Type = PromotionType.Gift, Value = 0, StartsAt = Now.AddDays(-1), EndsAt = Now.AddDays(1) };
        var bank = new Promotion { Name = "Bank", Type = PromotionType.BankOffer, Value = 500_000m, StartsAt = Now.AddDays(-1), EndsAt = Now.AddDays(1) };

        var result = PriceCalculator.Calculate(1, 1_000_000m, null, new[] { gift, bank }, Now);

        result.FinalPrice.Should().Be(1_000_000m);
        result.AppliedPromotionName.Should().BeNull();
    }
}
