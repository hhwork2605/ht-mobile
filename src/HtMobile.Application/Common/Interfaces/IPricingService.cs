using HtMobile.Application.Features.Pricing.Dtos;

namespace HtMobile.Application.Common.Interfaces;

/// <summary>
/// Pricing engine — module trung tâm (SPEC §2). Tính giá hiệu lực + khuyến mãi.
/// Hiện thực: <c>Features/Pricing/PricingEngine</c>.
/// </summary>
public interface IPricingService
{
    Task<EffectivePrice> GetEffectivePriceAsync(long variantId, CancellationToken ct = default);

    /// <summary>Xoá cache giá khi cập nhật giá/khuyến mãi của variant.</summary>
    Task InvalidateAsync(long variantId, CancellationToken ct = default);
}
