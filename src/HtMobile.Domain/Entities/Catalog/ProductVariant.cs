using HtMobile.Domain.Common;
using HtMobile.Domain.Entities.Inventory;
using HtMobile.Domain.Enums;

namespace HtMobile.Domain.Entities.Catalog;

/// <summary>Biến thể/SKU = 1 tổ hợp dung lượng × màu, có URL (slug) riêng. SPEC §5.</summary>
public class ProductVariant : BaseAuditableEntity
{
    public long ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Storage { get; set; }   // 256GB / 512GB / 1TB…
    public string? Color { get; set; }
    public string Slug { get; set; } = string.Empty;

    /// <summary>Giá bán niêm yết (giá duy nhất, không phân theo vùng).</summary>
    public decimal BasePrice { get; set; }

    /// <summary>Giá gạch ngang (giá gốc cao hơn) để hiển thị giảm giá; null = không hiển thị.</summary>
    public decimal? CompareAtPrice { get; set; }

    public VariantStatus Status { get; set; } = VariantStatus.Active;

    public Product Product { get; set; } = null!;
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Inventory.Inventory> Inventories { get; set; } = new List<Inventory.Inventory>();
}
