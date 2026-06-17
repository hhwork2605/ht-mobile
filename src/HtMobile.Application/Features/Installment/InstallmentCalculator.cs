namespace HtMobile.Application.Features.Installment;

/// <summary>Một phương án trả góp 0%: số kỳ + trả mỗi tháng + tổng (= giá, vì 0% lãi).</summary>
public readonly record struct InstallmentPlan(int Months, decimal MonthlyAmount, decimal Total);

/// <summary>
/// Tính trả góp 0% THUẦN (không DB) để dễ test. Trả/tháng = giá ÷ kỳ hạn, làm tròn LÊN đồng (kỳ cuối gánh phần lẻ).
/// </summary>
public static class InstallmentCalculator
{
    /// <summary>Các kỳ hạn (tháng) hỗ trợ.</summary>
    public static readonly int[] Terms = { 6, 9, 12 };

    /// <summary>Phương án trả góp cho từng kỳ hạn; rỗng nếu giá ≤ 0.</summary>
    public static IReadOnlyList<InstallmentPlan> Plans(decimal price)
    {
        if (price <= 0) return Array.Empty<InstallmentPlan>();
        return Terms.Select(m => new InstallmentPlan(m, Monthly(price, m), price)).ToList();
    }

    /// <summary>Trả/tháng thấp nhất (kỳ hạn dài nhất) — cho dòng "chỉ từ X/tháng". 0 nếu giá ≤ 0.</summary>
    public static decimal MonthlyFrom(decimal price)
        => price <= 0 ? 0m : Monthly(price, Terms.Max());

    private static decimal Monthly(decimal price, int months) => Math.Ceiling(price / months);
}
