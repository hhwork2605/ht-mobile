using System.Text.RegularExpressions;

namespace HtMobile.Application.Features.StockNotifications;

/// <summary>Kiểm tra contact theo dõi hàng về: hợp lệ nếu là email HOẶC số điện thoại VN. Thuần, dễ test.</summary>
public static class ContactValidator
{
    private static readonly Regex Email = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    // SĐT VN: 10 số bắt đầu '0' (0xxxxxxxxx) hoặc dạng quốc tế +84 + 9 số.
    private static readonly Regex VnPhone = new(@"^(0\d{9}|\+84\d{9})$", RegexOptions.Compiled);

    public static bool IsValid(string? contact)
    {
        if (string.IsNullOrWhiteSpace(contact)) return false;
        contact = contact.Trim();
        return Email.IsMatch(contact) || VnPhone.IsMatch(contact);
    }
}
