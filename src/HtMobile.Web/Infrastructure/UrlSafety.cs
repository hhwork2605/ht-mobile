namespace HtMobile.Web.Infrastructure;

/// <summary>
/// Bảo vệ ReturnUrl khỏi open-redirect: chỉ chấp nhận URL nội bộ (đường dẫn tương đối an toàn),
/// còn lại trả về <paramref name="fallback"/>. Logic thuần để unit-test.
/// </summary>
public static class UrlSafety
{
    /// <summary>Trả <paramref name="url"/> nếu là URL nội bộ an toàn; ngược lại trả <paramref name="fallback"/>.</summary>
    public static string SafeLocalUrl(string? url, string fallback = "/")
    {
        if (string.IsNullOrWhiteSpace(url)) return fallback;

        // Hợp lệ: bắt đầu bằng '/' nhưng KHÔNG phải '//' (protocol-relative) hay '/\' (mẹo backslash).
        // Loại: URL tuyệt đối (http://, javascript:…), URL không bắt đầu bằng '/'.
        if (url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\')))
            return url;

        return fallback;
    }
}
