using HtMobile.Domain.Common;
using AttributeEntity = HtMobile.Domain.Entities.Catalog.Attribute;

namespace HtMobile.Domain.Entities.Catalog;

/// <summary>
/// Giá trị 1 thuộc tính của 1 sản phẩm/biến thể (EAV). Vd biến thể "iPhone 256GB Đen":
/// {Attribute "Dung lượng" → "256GB"}, {Attribute "Màu" → "Đen"}. Unique (ProductId, AttributeId). Xem ADR 0003.
/// </summary>
public class ProductAttribute : BaseEntity
{
    public long AttributeId { get; set; }
    public long ProductId { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    public AttributeEntity Attribute { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
