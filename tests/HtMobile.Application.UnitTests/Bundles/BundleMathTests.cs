using FluentAssertions;
using HtMobile.Application.Features.Bundles;
using Xunit;

namespace HtMobile.Application.UnitTests.Bundles;

public class BundleMathTests
{
    [Fact]
    public void Empty_selection_is_zero()
    {
        var t = BundleMath.Summarize(Array.Empty<BundleLineInput>());
        t.TotalListed.Should().Be(0m);
        t.TotalBundle.Should().Be(0m);
        t.TotalSaving.Should().Be(0m);
    }

    [Fact]
    public void Sums_listed_bundle_and_saving()
    {
        var lines = new[]
        {
            new BundleLineInput(ListedPrice: 1_000_000m, BundlePrice: 800_000m),
            new BundleLineInput(ListedPrice: 500_000m, BundlePrice: 400_000m),
        };

        var t = BundleMath.Summarize(lines);

        t.TotalListed.Should().Be(1_500_000m);
        t.TotalBundle.Should().Be(1_200_000m);
        t.TotalSaving.Should().Be(300_000m);   // 1.5tr - 1.2tr
    }

    [Fact]
    public void Saving_never_negative_when_bundle_price_higher()
    {
        var lines = new[] { new BundleLineInput(ListedPrice: 100_000m, BundlePrice: 150_000m) };

        var t = BundleMath.Summarize(lines);

        t.TotalSaving.Should().Be(0m);
    }
}
