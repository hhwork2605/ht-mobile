using HtMobile.Domain.Common;

namespace HtMobile.Domain.Entities.Pricing;

/// <summary>Khu vực giá: miền Bắc / miền Nam. SPEC §1, §7.</summary>
public class Region : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;   // "north" / "south"

    public ICollection<PriceByRegion> Prices { get; set; } = new List<PriceByRegion>();
}
