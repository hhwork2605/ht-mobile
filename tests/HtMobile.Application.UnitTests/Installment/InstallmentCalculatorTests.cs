using FluentAssertions;
using HtMobile.Application.Features.Installment;
using Xunit;

namespace HtMobile.Application.UnitTests.Installment;

public class InstallmentCalculatorTests
{
    [Fact]
    public void Plans_cover_all_terms_with_zero_interest()
    {
        var plans = InstallmentCalculator.Plans(12_000_000m);

        plans.Should().HaveCount(3);
        plans.Select(p => p.Months).Should().BeEquivalentTo(new[] { 6, 9, 12 });
        plans.Should().OnlyContain(p => p.Total == 12_000_000m);     // 0% → tổng = giá
        plans.Single(p => p.Months == 6).MonthlyAmount.Should().Be(2_000_000m);
        plans.Single(p => p.Months == 12).MonthlyAmount.Should().Be(1_000_000m);
    }

    [Fact]
    public void Monthly_is_rounded_up_to_whole_dong()
    {
        // 1.000.000 / 6 = 166.666,67 → làm tròn LÊN = 166.667
        var plans = InstallmentCalculator.Plans(1_000_000m);
        plans.Single(p => p.Months == 6).MonthlyAmount.Should().Be(166_667m);
    }

    [Fact]
    public void MonthlyFrom_uses_longest_term()
    {
        InstallmentCalculator.MonthlyFrom(12_000_000m).Should().Be(1_000_000m);   // 12 tháng
    }

    [Fact]
    public void MonthlyFrom_rounds_up_on_non_divisible_price()
    {
        // 10.000.000 / 12 = 833.333,33 → làm tròn LÊN = 833.334
        InstallmentCalculator.MonthlyFrom(10_000_000m).Should().Be(833_334m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Non_positive_price_has_no_plans(decimal price)
    {
        InstallmentCalculator.Plans(price).Should().BeEmpty();
        InstallmentCalculator.MonthlyFrom(price).Should().Be(0m);
    }
}
