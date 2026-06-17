namespace HtMobile.Application.Features.TradeIn;

/// <summary>Ước tính thu cũ: giá thu (theo tình trạng) + trợ giá + tổng nhận.</summary>
public readonly record struct TradeInQuote(decimal TradeInValue, decimal Subsidy, decimal Total);

/// <summary>Định giá thu cũ THUẦN (không DB) để dễ test. Giá thu = round(base × hệ số); tổng = giá thu + trợ giá.</summary>
public static class TradeInEstimator
{
    public static TradeInQuote Estimate(decimal baseValue, decimal conditionFactor, decimal subsidy)
    {
        if (baseValue < 0) baseValue = 0;
        var tradeIn = Math.Round(baseValue * conditionFactor, 0, MidpointRounding.AwayFromZero);
        if (tradeIn < 0) tradeIn = 0;
        if (subsidy < 0) subsidy = 0;
        return new TradeInQuote(tradeIn, subsidy, tradeIn + subsidy);
    }
}
