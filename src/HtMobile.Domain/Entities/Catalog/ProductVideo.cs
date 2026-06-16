using HtMobile.Domain.Common;

namespace HtMobile.Domain.Entities.Catalog;

public class ProductVideo : BaseEntity
{
    public long ProductId { get; set; }
    public string YoutubeUrl { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}
