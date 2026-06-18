namespace HtMobile.Application.Features.Catalog.Dtos;

/// <summary>1 dòng danh mục trong bảng admin.</summary>
public record AdminCategoryRow
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? ParentName { get; init; }
    public int SortOrder { get; init; }
    public int ProductCount { get; init; }
    public int ChildCount { get; init; }
}

/// <summary>Chi tiết danh mục cho form sửa.</summary>
public record AdminCategoryDetail
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public long? ParentId { get; init; }
    public int SortOrder { get; init; }
    public string? SeoContent { get; init; }
}

/// <summary>Input tạo/sửa danh mục. Slug trống → tự sinh từ Name.</summary>
public record CategoryInput(string Name, string? Slug, long? ParentId, int SortOrder, string? SeoContent);

/// <summary>Kết quả thao tác danh mục.</summary>
public enum AdminCategoryResult { Ok, NotFound, SlugExists, InvalidParent, HasChildren, HasProducts }
