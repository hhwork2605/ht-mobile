using FluentAssertions;
using HtMobile.Application.Common;
using Xunit;

namespace HtMobile.Application.UnitTests.Catalog;

public class SlugifyTests
{
    [Theory]
    [InlineData("iPhone 17 Pro Max", "iphone-17-pro-max")]
    [InlineData("Áo Thun Đỏ", "ao-thun-do")]
    [InlineData("Tai nghe  ABC!!!", "tai-nghe-abc")]
    [InlineData("Điện thoại — 256GB", "dien-thoai-256gb")]
    [InlineData("Mac mini (M4)", "mac-mini-m4")]
    public void Generates_ascii_lowercase_hyphen_slug(string name, string expected)
    {
        Slugify.ToSlug(name).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!!")]
    public void Empty_or_symbol_only_gives_empty(string? name)
    {
        Slugify.ToSlug(name).Should().Be("");
    }
}
