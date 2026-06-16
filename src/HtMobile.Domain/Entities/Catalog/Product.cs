using HtMobile.Domain.Common;

namespace HtMobile.Domain.Entities.Catalog;

/// <summary>Sản phẩm (model). Mỗi tổ hợp dung lượng×màu là 1 <see cref="ProductVariant"/>.</summary>
public class Product : BaseAuditableEntity
{
    public long CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    /// <summary>Khẩu hiệu ngắn hiển thị trên ProductCard, vd "Titan. Mạnh mẽ. Chuyên nghiệp."</summary>
    public string? Tagline { get; set; }
    public string? Description { get; set; }
    public string? Brand { get; set; }

    /// <summary>Thông số kỹ thuật lưu dạng jsonb.</summary>
    public string? SpecsJson { get; set; }

    public Category Category { get; set; } = null!;
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductVideo> Videos { get; set; } = new List<ProductVideo>();
}
