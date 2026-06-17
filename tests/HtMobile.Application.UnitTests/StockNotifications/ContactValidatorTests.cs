using FluentAssertions;
using HtMobile.Application.Features.StockNotifications;
using Xunit;

namespace HtMobile.Application.UnitTests.StockNotifications;

public class ContactValidatorTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("nguyen.van.a@htmobile.vn")]
    [InlineData("0901234567")]
    [InlineData("+84901234567")]
    public void Valid_email_or_vn_phone(string contact)
    {
        ContactValidator.IsValid(contact).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("a@b")]              // thiếu TLD
    [InlineData("12345")]            // quá ngắn / không đúng định dạng SĐT
    [InlineData("0123")]
    [InlineData("09012345678")]      // 0 + 10 số = 11 ký tự (không hợp lệ)
    [InlineData("+8490123456")]      // +84 + 8 số (thiếu)
    public void Invalid_contact(string? contact)
    {
        ContactValidator.IsValid(contact).Should().BeFalse();
    }
}
