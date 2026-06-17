using HtMobile.Domain.Enums;

namespace HtMobile.Application.Features.Pricing.Dtos;

/// <summary>1 dòng khuyến mãi trong bảng admin.</summary>
public record AdminPromotionRow
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public PromotionType Type { get; init; }
    public decimal Value { get; init; }
    public DateTime StartsAt { get; init; }
    public DateTime EndsAt { get; init; }
    public bool IsActiveNow { get; init; }
}

/// <summary>Chi tiết khuyến mãi cho form sửa.</summary>
public record AdminPromotionDetail
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public PromotionType Type { get; init; }
    public decimal Value { get; init; }
    public DateTime StartsAt { get; init; }
    public DateTime EndsAt { get; init; }
    public string? ConditionsJson { get; init; }
}

/// <summary>Input tạo/sửa khuyến mãi.</summary>
public record PromotionInput(string Name, PromotionType Type, decimal Value, DateTime StartsAt, DateTime EndsAt, string? ConditionsJson);
