using FluentAssertions;
using HtMobile.Application.Features.Pricing;
using Xunit;

namespace HtMobile.Application.UnitTests.Pricing;

public class PromotionConditionsTests
{
    // biến thể (con) 100, model cha 10, category 1
    private static readonly PricingContext Ctx = new(ProductId: 100, ParentProductId: 10, CategoryId: 1);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{}")]
    [InlineData("{\"categoryIds\":[],\"productIds\":[],\"variantIds\":[]}")]
    public void Empty_conditions_apply_to_all(string? json)
    {
        PromotionConditions.Matches(json, Ctx).Should().BeTrue();
    }

    [Theory]
    [InlineData("{\"categoryIds\":[1]}")]      // khớp category
    [InlineData("{\"productIds\":[10]}")]      // khớp product
    [InlineData("{\"variantIds\":[100]}")]     // khớp variant
    [InlineData("{\"categoryIds\":[9],\"productIds\":[10]}")] // khớp 1 trong các danh sách
    public void Matches_when_context_in_a_declared_list(string json)
    {
        PromotionConditions.Matches(json, Ctx).Should().BeTrue();
    }

    [Theory]
    [InlineData("{\"categoryIds\":[2]}")]
    [InlineData("{\"productIds\":[11]}")]
    [InlineData("{\"variantIds\":[101]}")]
    [InlineData("{\"categoryIds\":[2],\"variantIds\":[101]}")]
    public void No_match_when_declared_lists_exclude_context(string json)
    {
        PromotionConditions.Matches(json, Ctx).Should().BeFalse();
    }

    [Fact]
    public void Malformed_json_does_not_apply()
    {
        PromotionConditions.Matches("{not json", Ctx).Should().BeFalse();
    }

    [Fact]
    public void Null_element_in_array_does_not_apply()
    {
        // [1,null] không hợp lệ cho long[] → JsonException → không áp (an toàn).
        PromotionConditions.Matches("{\"categoryIds\":[1,null]}", Ctx).Should().BeFalse();
    }

    [Fact]
    public void Unknown_fields_are_ignored()
    {
        PromotionConditions.Matches("{\"foo\":[9],\"categoryIds\":[1]}", Ctx).Should().BeTrue();
    }
}
