using HtMobile.Domain.Common;

namespace HtMobile.Domain.Entities.Catalog;

/// <summary>
/// Định nghĩa loại thuộc tính sản phẩm (master), vd "Dung lượng", "Màu", "RAM". Giá trị cụ thể cho từng
/// sản phẩm/biến thể lưu ở <see cref="ProductAttribute"/>. Xem ADR 0003.
/// </summary>
public class Attribute : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public ICollection<ProductAttribute> ProductAttributes { get; set; } = new List<ProductAttribute>();
}
