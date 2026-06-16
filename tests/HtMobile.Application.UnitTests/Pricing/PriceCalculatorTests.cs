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
        var result = PriceCalculator.Calculate(1, 1, listPrice: 1_000_000m, compareAtPrice: null,
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

        var result = PriceCalculator.Calculate(1, 1, listPrice: 1_000_000m, compareAtPrice: 1_200_000m,
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

        var result = PriceCalculator.Calculate(1, 1, 1_000_000m, null, new[] { promo }, Now);

        result.FinalPrice.Should().Be(1_000_000m);
    }

    [Fact]
    public void Best_discount_wins_among_multiple()
    {
        var p10 = new Promotion { Name = "10%", Type = PromotionType.Percentage, Value = 10, StartsAt = Now.AddDays(-1), EndsAt = Now.AddDays(1) };
        var pFixed = new Promotion { Name = "-300k", Type = PromotionType.FixedAmount, Value = 300_000m, StartsAt = Now.AddDays(-1), EndsAt = Now.AddDays(1) };

        var result = PriceCalculator.Calculate(1, 1, 1_000_000m, null, new[] { p10, pFixed }, Now);

        result.FinalPrice.Should().Be(700_000m);
        result.AppliedPromotionName.Should().Be("-300k");
    }
}
