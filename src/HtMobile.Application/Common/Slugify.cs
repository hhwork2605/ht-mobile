using System.Globalization;
using System.Text;

namespace HtMobile.Application.Common;

/// <summary>Sinh slug SEO từ tên (tiếng Việt → ascii, thường, gạch nối). Thuần, dễ test.</summary>
public static class Slugify
{
    public static string ToSlug(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;

        // đ/Đ không tách dấu được qua Normalize → thay tay trước.
        var s = name.Replace('Đ', 'D').Replace('đ', 'd');

        // Tách dấu (combining marks) rồi bỏ chúng: "ế" → "e".
        var decomposed = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(c);
        }

        // Còn lại: thường hoá; [a-z0-9] giữ nguyên, mọi thứ khác → gạch nối (gộp liên tiếp).
        var ascii = sb.ToString().ToLowerInvariant();
        var slug = new StringBuilder(ascii.Length);
        var pendingHyphen = false;
        foreach (var c in ascii)
        {
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
            {
                if (pendingHyphen && slug.Length > 0) slug.Append('-');
                pendingHyphen = false;
                slug.Append(c);
            }
            else
            {
                pendingHyphen = true;
            }
        }
        return slug.ToString();
    }
}
