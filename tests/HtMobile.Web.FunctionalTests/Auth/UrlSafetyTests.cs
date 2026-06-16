using FluentAssertions;
using HtMobile.Web.Infrastructure;
using Xunit;

namespace HtMobile.Web.FunctionalTests.Auth;

public class UrlSafetyTests
{
    [Theory]
    [InlineData("/cart")]
    [InlineData("/dien-thoai-iphone-17-pro-max-256gb")]
    [InlineData("/account?tab=info")]
    public void Local_paths_are_kept(string url)
    {
        UrlSafety.SafeLocalUrl(url).Should().Be(url);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("https://evil.com")]          // absolute → chặn
    [InlineData("//evil.com")]                // protocol-relative → chặn
    [InlineData("/\\evil.com")]               // backslash trick → chặn
    [InlineData("javascript:alert(1)")]       // scheme khác → chặn
    [InlineData("evil.com")]                  // không bắt đầu bằng "/" → chặn
    public void Non_local_or_empty_falls_back_to_root(string? url)
    {
        UrlSafety.SafeLocalUrl(url).Should().Be("/");
    }

    [Fact]
    public void Uses_custom_fallback()
    {
        UrlSafety.SafeLocalUrl("https://evil.com", "/login").Should().Be("/login");
    }
}
