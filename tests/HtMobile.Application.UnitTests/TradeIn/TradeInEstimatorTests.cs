using FluentAssertions;
using HtMobile.Application.Features.TradeIn;
using Xunit;

namespace HtMobile.Application.UnitTests.TradeIn;

public class TradeInEstimatorTests
{
    [Fact]
    public void Estimate_applies_factor_and_adds_subsidy()
    {
        var q = TradeInEstimator.Estimate(baseValue: 10_000_000m, conditionFactor: 0.9m, subsidy: 500_000m);

        q.TradeInValue.Should().Be(9_000_000m);   // 10tr × 0.9
        q.Subsidy.Should().Be(500_000m);
        q.Total.Should().Be(9_500_000m);           // giá thu + trợ giá
    }

    [Fact]
    public void Trade_in_value_is_rounded_to_whole_dong()
    {
        // 999.999 × 0.5 = 499.999,5 → làm tròn = 500.000
        var q = TradeInEstimator.Estimate(999_999m, 0.5m, 0m);
        q.TradeInValue.Should().Be(500_000m);
        (q.TradeInValue % 1m).Should().Be(0m);
    }

    [Fact]
    public void Catalog_lookup_returns_null_for_unknown_key()
    {
        TradeInCatalog.FindDevice("khong-co").Should().BeNull();
        TradeInCatalog.FindCondition("xxx").Should().BeNull();
        TradeInCatalog.FindDevice("iphone-13").Should().NotBeNull();
    }
}
