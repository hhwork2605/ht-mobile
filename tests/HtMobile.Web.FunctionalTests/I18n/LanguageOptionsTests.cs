using FluentAssertions;
using HtMobile.Web.Infrastructure;
using Xunit;

namespace HtMobile.Web.FunctionalTests.I18n;

public class LanguageOptionsTests
{
    [Theory]
    [InlineData("vi", "vi")]
    [InlineData("en", "en")]
    [InlineData("EN", "en")]     // không phân biệt hoa thường
    [InlineData("Vi", "vi")]
    public void Resolve_keeps_supported_culture(string code, string expected)
    {
        LanguageOptions.Resolve(code).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("fr")]
    [InlineData("xx-YY")]
    public void Resolve_falls_back_to_default(string? code)
    {
        LanguageOptions.Resolve(code).Should().Be("vi");
    }

    [Theory]
    [InlineData("vi", true)]
    [InlineData("en", true)]
    [InlineData("fr", false)]
    [InlineData(null, false)]
    public void IsSupported_reflects_supported_set(string? code, bool expected)
    {
        LanguageOptions.IsSupported(code).Should().Be(expected);
    }
}
