namespace HtMobile.Application.Features.Catalog;

/// <summary>Quy tắc badge cho ProductCard (logic thuần, dễ unit-test).</summary>
public static class CatalogBadge
{
    /// <summary>Số ngày coi là "Mới" kể từ lúc tạo sản phẩm.</summary>
    public const int NewWindowDays = 30;

    /// <summary>Sản phẩm tạo trong vòng <paramref name="days"/> ngày gần đây được gắn nhãn "Mới".</summary>
    public static bool IsNew(DateTime createdAt, DateTime now, int days = NewWindowDays)
        => createdAt >= now.AddDays(-days);
}
