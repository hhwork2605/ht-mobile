using FluentAssertions;
using HtMobile.Application.Features.Cart;
using Xunit;

namespace HtMobile.Application.UnitTests.Cart;

public class CartMathTests
{
    [Fact]
    public void Summarize_empty_cart_is_all_zero()
    {
        var totals = CartMath.Summarize(Array.Empty<CartLineAmount>());

        totals.Count.Should().Be(0);
        totals.Subtotal.Should().Be(0m);
        totals.Total.Should().Be(0m);
    }

    [Fact]
    public void Summarize_sums_quantity_and_line_totals()
    {
        var lines = new[]
        {
            new CartLineAmount(1_000_000m, 2),   // 2.000.000
            new CartLineAmount(500_000m, 3),     // 1.500.000
        };

        var totals = CartMath.Summarize(lines);

        totals.Count.Should().Be(5);
        totals.Subtotal.Should().Be(3_500_000m);
        totals.ShippingFee.Should().Be(0m);            // Phase 2: miễn phí
        totals.Total.Should().Be(3_500_000m);          // = tạm tính + ship
    }

    [Theory]
    [InlineData(1, 1, 2)]    // tăng
    [InlineData(3, -1, 2)]   // giảm
    [InlineData(1, -1, 0)]   // về 0
    [InlineData(1, -5, 0)]   // không âm
    public void NextQuantity_clamps_at_zero(int current, int delta, int expected)
    {
        CartMath.NextQuantity(current, delta).Should().Be(expected);
    }

    [Fact]
    public void Merge_combines_quantities_by_variant()
    {
        var existing = new[] { (VariantId: 1L, Quantity: 1), (VariantId: 2L, Quantity: 2) };
        var incoming = new[] { (VariantId: 2L, Quantity: 3), (VariantId: 3L, Quantity: 1) };

        var merged = CartMath.Merge(existing, incoming);

        merged.Should().HaveCount(3);
        merged.Single(x => x.VariantId == 1).Quantity.Should().Be(1);
        merged.Single(x => x.VariantId == 2).Quantity.Should().Be(5);   // 2 + 3
        merged.Single(x => x.VariantId == 3).Quantity.Should().Be(1);
    }
}
