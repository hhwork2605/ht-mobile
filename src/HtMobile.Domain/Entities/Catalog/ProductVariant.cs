using HtMobile.Domain.Common;
using HtMobile.Domain.Entities.Inventory;
using HtMobile.Domain.Entities.Pricing;
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
    public decimal BasePrice { get; set; }
    public VariantStatus Status { get; set; } = VariantStatus.Active;

    public Product Product { get; set; } = null!;
    public ICollection<PriceByRegion> Prices { get; set; } = new List<PriceByRegion>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Inventory.Inventory> Inventories { get; set; } = new List<Inventory.Inventory>();
}
