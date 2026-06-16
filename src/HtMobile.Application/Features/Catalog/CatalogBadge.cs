namespace HtMobile.Application.Features.Catalog;

/// <summary>Quy tắc badge cho ProductCard (logic thuần, dễ unit-test).</summary>
public static class CatalogBadge
{
    /// <summary>Số ngày coi là "Mới" kể từ lúc tạo sản phẩm.</summary>
    public const int NewWindowDays = 30;

    /// <summary>Sản phẩm tạo trong vòng <paramref name="days"/> ngày gần đây được gắn nhãn "Mới".</summary>
    public static bool IsNew(DateTime createdAt, DateTime now, int days = NewWindowDays)
        => createdAt >= now.AddDays(-days);

    /// <summary>
    /// Suy ra "dòng máy" (series) từ tên sản phẩm để gom tab lọc trên trang danh mục —
    /// lấy 2 từ đầu (vd "iPhone 17 Pro Max" → "iPhone 17", "MacBook Air M3 13 inch" → "MacBook Air").
    /// Phase 1 dùng heuristic này; có thể thay bằng trường Series riêng khi catalog lớn hơn.
    /// </summary>
    public static string Series(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length <= 2 ? name.Trim() : $"{parts[0]} {parts[1]}";
    }
}
