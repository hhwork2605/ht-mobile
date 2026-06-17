namespace HtMobile.Application.Features.Bundles;

/// <summary>Một phụ kiện mua kèm: giá niêm yết (qua IPricingService) vs giá mua kèm (BundleItem.BundlePrice).</summary>
public readonly record struct BundleLineInput(decimal ListedPrice, decimal BundlePrice);

/// <summary>Tổng tiền + tiết kiệm của các phụ kiện mua kèm (KHÔNG gồm SP chính).</summary>
public readonly record struct BundleTotals(decimal TotalListed, decimal TotalBundle, decimal TotalSaving);

/// <summary>Tính tổng/tiết kiệm bundle THUẦN (không DB) để dễ test.</summary>
public static class BundleMath
{
    public static BundleTotals Summarize(IEnumerable<BundleLineInput> accessories)
    {
        decimal listed = 0m, bundle = 0m;
        foreach (var a in accessories)
        {
            listed += a.ListedPrice;
            bundle += a.BundlePrice;
        }
        var saving = listed - bundle;
        if (saving < 0) saving = 0m;
        return new BundleTotals(listed, bundle, saving);
    }
}
