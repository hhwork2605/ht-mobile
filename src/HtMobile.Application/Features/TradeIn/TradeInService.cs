using HtMobile.Application.Common.Interfaces;
using HtMobile.Domain.Entities.Services;
using HtMobile.Domain.Enums;

namespace HtMobile.Application.Features.TradeIn;

/// <summary>Kết quả gửi yêu cầu thu cũ (id + báo giá ước tính).</summary>
public record TradeInResult(long RequestId, TradeInQuote Quote);

/// <summary>Thu cũ đổi mới (P3-03): định giá + tạo yêu cầu. Estimate tính lại server-side từ catalog (không tin client).</summary>
public class TradeInService
{
    private readonly IApplicationDbContext _db;

    public TradeInService(IApplicationDbContext db) => _db = db;

    /// <summary>Gửi yêu cầu thu cũ. Trả null nếu dòng máy/tình trạng không hợp lệ (không tạo request).</summary>
    public async Task<TradeInResult?> SubmitAsync(long? customerId, string? deviceKey, string? conditionKey, CancellationToken ct = default)
    {
        var device = TradeInCatalog.FindDevice(deviceKey);
        var condition = TradeInCatalog.FindCondition(conditionKey);
        if (device is null || condition is null) return null;

        var quote = TradeInEstimator.Estimate(device.BaseValue, condition.Factor, TradeInCatalog.Subsidy);

        var request = new TradeInRequest
        {
            CustomerId = customerId,
            Model = device.Name,
            Condition = condition.Label,
            // EstimatedPrice = TỔNG ước tính khách nhận = giá thu + trợ giá (quote.Total), không chỉ giá thu.
            EstimatedPrice = quote.Total,
            Status = TradeInStatus.Pending
        };
        _db.TradeInRequests.Add(request);
        await _db.SaveChangesAsync(ct);

        return new TradeInResult(request.Id, quote);
    }
}
