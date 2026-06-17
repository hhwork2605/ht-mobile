using HtMobile.Application.Features.StockNotifications;

namespace HtMobile.Web.Models.Stock;

public class StockNotifyVm
{
    public long VariantId { get; set; }
    public string? Contact { get; set; }
    /// <summary>null = form ban đầu (chưa submit).</summary>
    public StockNotifyResult? Result { get; set; }
}
