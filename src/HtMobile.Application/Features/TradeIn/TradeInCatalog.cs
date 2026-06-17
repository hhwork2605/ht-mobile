namespace HtMobile.Application.Features.TradeIn;

/// <summary>Dòng máy cũ định sẵn để định giá thu (giá base ước tính khi tình trạng "như mới").</summary>
public record TradeInDevice(string Key, string Name, decimal BaseValue);

/// <summary>Tình trạng máy + hệ số nhân vào giá base.</summary>
public record TradeInConditionOption(string Key, string Label, decimal Factor);

/// <summary>Danh mục thu cũ tĩnh (Phase 3 — không entity/DB). Thay bằng cấu hình động ở phase sau.</summary>
public static class TradeInCatalog
{
    /// <summary>Trợ giá lên đời (bonus cố định cộng vào giá thu).</summary>
    public const decimal Subsidy = 500_000m;

    public static IReadOnlyList<TradeInDevice> Devices { get; } = new[]
    {
        new TradeInDevice("iphone-15-pro-max", "iPhone 15 Pro Max", 22_000_000m),
        new TradeInDevice("iphone-15", "iPhone 15", 15_000_000m),
        new TradeInDevice("iphone-14", "iPhone 14", 10_000_000m),
        new TradeInDevice("iphone-13", "iPhone 13", 7_000_000m),
        new TradeInDevice("other", "Khác", 2_000_000m),
    };

    public static IReadOnlyList<TradeInConditionOption> Conditions { get; } = new[]
    {
        new TradeInConditionOption("tot", "Tốt (như mới)", 0.9m),
        new TradeInConditionOption("kha", "Khá (trầy xước nhẹ)", 0.7m),
        new TradeInConditionOption("tb", "Trung bình", 0.5m),
    };

    public static TradeInDevice? FindDevice(string? key)
        => Devices.FirstOrDefault(d => d.Key == key);

    public static TradeInConditionOption? FindCondition(string? key)
        => Conditions.FirstOrDefault(c => c.Key == key);
}
