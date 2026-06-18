namespace HtMobile.Web.Infrastructure;

/// <summary>
/// Ánh xạ slug danh mục → glyph "KV Icon Kit" (class <c>ik-*</c>) dùng cho storefront.
/// Theme ShopDunk dùng bộ icon kv-icons (iks/ikr/ikb), không dùng Font Awesome.
/// </summary>
public static class StorefrontIcons
{
    /// <summary>Glyph cho 1 danh mục theo slug. Mặc định <c>ik-tag</c> nếu chưa map.</summary>
    public static string Category(string? slug) => slug switch
    {
        "iphone" => "ik-mobile",
        "ipad" => "ik-mobile-screen",
        "mac" => "ik-laptop",
        "apple-watch" or "watch" => "ik-clock",
        "phu-kien" or "phukien" => "ik-volume",
        "am-thanh" => "ik-volume",
        "camera" => "ik-camera",
        "gia-dung" => "ik-blender",
        "may-cu" => "ik-arrows-rotate",
        _ => "ik-tag",
    };
}
