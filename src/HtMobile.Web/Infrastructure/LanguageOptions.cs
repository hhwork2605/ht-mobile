namespace HtMobile.Web.Infrastructure;

/// <summary>Ngôn ngữ hỗ trợ + chuẩn hoá culture code (thuần, dễ test). VI mặc định, EN tuỳ chọn.</summary>
public static class LanguageOptions
{
    public const string Default = "vi";
    public static readonly string[] Supported = { "vi", "en" };

    /// <summary>Culture code có được hỗ trợ không (không phân biệt hoa thường).</summary>
    public static bool IsSupported(string? code)
        => !string.IsNullOrWhiteSpace(code) && Supported.Contains(code.Trim().ToLowerInvariant());

    /// <summary>Trả culture hợp lệ (chuẩn hoá thường); code lạ/null → <see cref="Default"/>.</summary>
    public static string Resolve(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return Default;
        var c = code.Trim().ToLowerInvariant();
        return Supported.Contains(c) ? c : Default;
    }
}
