using HtMobile.Application.Features.Pricing.Dtos;

namespace HtMobile.Application.Common.Interfaces;

/// <summary>
/// Pricing engine — module trung tâm (SPEC §2). Tính giá hiệu lực + khuyến mãi.
/// Hiện thực: <c>Features/Pricing/PricingEngine</c>.
/// </summary>
public interface IPricingService
{
    /// <summary>Giá hiệu lực của 1 biến thể (Product con). productId = Id của Product bán thực sự.</summary>
    Task<EffectivePrice> GetEffectivePriceAsync(long productId, CancellationToken ct = default);

    /// <summary>Xoá cache giá khi cập nhật giá/khuyến mãi của 1 Product (biến thể).</summary>
    Task InvalidateAsync(long productId, CancellationToken ct = default);
}
