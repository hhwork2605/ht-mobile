using HtMobile.Domain.Common;

namespace HtMobile.Domain.Entities.Inventory;

/// <summary>Cửa hàng trong chuỗi (định vị + tồn kho theo cửa hàng). SPEC §4.2, §7.</summary>
public class Store : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public long? RegionId { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }
    public string? Phone { get; set; }

    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
}
