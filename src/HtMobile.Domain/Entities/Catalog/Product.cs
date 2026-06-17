using HtMobile.Domain.Common;
using HtMobile.Domain.Enums;

namespace HtMobile.Domain.Entities.Catalog;

/// <summary>
/// Sản phẩm. Tự tham chiếu qua <see cref="ProductParentId"/>: null = sản phẩm cha (model, gom + hiển thị),
/// có giá trị = biến thể con (đơn vị bán: có giá/SKU/tồn kho). Thuộc tính (dung lượng, màu, RAM…) lưu ở
/// <see cref="ProductAttribute"/> (EAV). Thay cho entity ProductVariant cũ — xem ADR 0003.
/// </summary>
public class Product : BaseAuditableEntity
{
    public long CategoryId { get; set; }

    /// <summary>null = sản phẩm cha (model). Có giá trị = biến thể con của model này.</summary>
    public long? ProductParentId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    /// <summary>Mã SKU (chỉ đặt cho biến thể con bán thực sự); unique khi NOT NULL.</summary>
    public string? Sku { get; set; }

    /// <summary>Giá bán niêm yết của biến thể (con). Cha thường để 0.</summary>
    public decimal BasePrice { get; set; }

    /// <summary>Giá gạch ngang (giá gốc cao hơn) để hiển thị giảm giá; null = không hiển thị.</summary>
    public decimal? CompareAtPrice { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Active;

    /// <summary>Khẩu hiệu ngắn hiển thị trên ProductCard, vd "Titan. Mạnh mẽ. Chuyên nghiệp."</summary>
    public string? Tagline { get; set; }
    public string? Description { get; set; }
    public string? Brand { get; set; }

    /// <summary>Thông số kỹ thuật lưu dạng jsonb.</summary>
    public string? SpecsJson { get; set; }

    public Category Category { get; set; } = null!;

    public Product? Parent { get; set; }
    public ICollection<Product> Children { get; set; } = new List<Product>();

    public ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductVideo> Videos { get; set; } = new List<ProductVideo>();
}
