using HtMobile.Domain.Common;

namespace HtMobile.Domain.Entities.Catalog;

/// <summary>Danh mục sản phẩm dạng cây (iPhone, iPad, Mac…). SPEC §7.</summary>
public class Category : BaseAuditableEntity
{
    public long? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string? SeoContent { get; set; }

    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
