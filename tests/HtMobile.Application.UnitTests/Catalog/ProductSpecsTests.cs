using FluentAssertions;
using HtMobile.Application.Features.Catalog;
using Xunit;

namespace HtMobile.Application.UnitTests.Catalog;

public class ProductSpecsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("not json")]
    [InlineData("123")]
    public void Empty_or_invalid_json_yields_no_groups(string? json)
    {
        ProductSpecs.Parse(json).Should().BeEmpty();
    }

    [Fact]
    public void Flat_array_of_items_becomes_one_untitled_group()
    {
        var json = """[{"label":"Màn hình","value":"6.9\""},{"label":"Chip","value":"A18 Pro"}]""";

        var groups = ProductSpecs.Parse(json);

        groups.Should().HaveCount(1);
        groups[0].Group.Should().BeNull();
        groups[0].Items.Should().HaveCount(2);
        groups[0].Items[0].Should().BeEquivalentTo(new { Label = "Màn hình", Value = "6.9\"" });
        groups[0].Items[1].Label.Should().Be("Chip");
    }

    [Fact]
    public void Grouped_array_keeps_group_titles_and_order()
    {
        var json = """
        [
          { "group": "Tổng quan", "items": [ { "label": "Chip", "value": "M3" } ] },
          { "group": "Pin", "items": [ { "label": "Thời lượng", "value": "18 giờ" } ] }
        ]
        """;

        var groups = ProductSpecs.Parse(json);

        groups.Should().HaveCount(2);
        groups[0].Group.Should().Be("Tổng quan");
        groups[1].Group.Should().Be("Pin");
        groups[1].Items[0].Value.Should().Be("18 giờ");
    }

    [Fact]
    public void Pair_array_is_supported()
    {
        var json = """[["Màn hình","6.1\""],["Chip","A18"]]""";

        var groups = ProductSpecs.Parse(json);

        groups.Should().ContainSingle();
        groups[0].Items.Should().HaveCount(2);
        groups[0].Items[0].Label.Should().Be("Màn hình");
    }

    [Fact]
    public void Object_map_is_supported()
    {
        var json = """{"Màn hình":"6.1\"","Chip":"A18"}""";

        var groups = ProductSpecs.Parse(json);

        groups.Should().ContainSingle();
        groups[0].Items.Should().HaveCount(2);
    }

    [Fact]
    public void Items_without_label_are_skipped()
    {
        var json = """[{"value":"orphan"},{"label":"Chip","value":"A18"}]""";

        var groups = ProductSpecs.Parse(json);

        groups[0].Items.Should().ContainSingle();
        groups[0].Items[0].Label.Should().Be("Chip");
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("[]", true)]
    [InlineData("""[{"label":"x","value":"y"}]""", true)]
    [InlineData("not json", false)]
    [InlineData("\"a string\"", false)]
    public void IsValid_reflects_json_validity(string? json, bool expected)
    {
        ProductSpecs.IsValid(json).Should().Be(expected);
    }
}
