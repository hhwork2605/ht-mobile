using HtMobile.Application.Features.Catalog.Dtos;

namespace HtMobile.Application.Features.Catalog;

/// <summary>1 thuộc tính của biến thể: tên + thứ tự + giá trị.</summary>
public readonly record struct VariantAttr(string Name, int SortOrder, string Value);

/// <summary>Thông tin 1 biến thể (Product con) để dựng bộ chọn theo trục.</summary>
public sealed record VariantInfo(long Id, string Slug, bool Active, IReadOnlyList<VariantAttr> Attrs);

/// <summary>
/// Dựng bộ chọn biến thể tách theo từng trục thuộc tính (THUẦN, dễ test). Mỗi thuộc tính (Dung lượng, Màu…)
/// thành 1 trục; mỗi giá trị là 1 lựa chọn trỏ tới biến thể phù hợp nhất: giữ nguyên các trục khác theo
/// biến thể đang chọn, chỉ đổi trục hiện tại (nếu tổ hợp không tồn tại → biến thể bất kỳ có giá trị đó).
/// </summary>
public static class VariantAxisBuilder
{
    public static IReadOnlyList<VariantAxisDto> Build(IReadOnlyList<VariantInfo> variants, long selectedId)
    {
        if (variants.Count == 0) return Array.Empty<VariantAxisDto>();

        var selected = variants.FirstOrDefault(v => v.Id == selectedId) ?? variants[0];

        static string? ValueOf(VariantInfo v, string name)
            => v.Attrs.FirstOrDefault(a => a.Name == name).Value is { Length: > 0 } s ? s : null;

        // Tên các trục theo thứ tự SortOrder (rồi tên), lấy từ mọi biến thể.
        var axisNames = variants
            .SelectMany(v => v.Attrs)
            .GroupBy(a => a.Name)
            .OrderBy(g => g.Min(a => a.SortOrder)).ThenBy(g => g.Key)
            .Select(g => g.Key)
            .ToList();

        var axes = new List<VariantAxisDto>(axisNames.Count);
        foreach (var name in axisNames)
        {
            // Giá trị theo thứ tự xuất hiện (giữ ổn định).
            var values = new List<string>();
            foreach (var v in variants)
            {
                var val = ValueOf(v, name);
                if (val is not null && !values.Contains(val)) values.Add(val);
            }

            var options = new List<VariantAxisOptionDto>(values.Count);
            foreach (var value in values)
            {
                // Ưu tiên biến thể giữ nguyên các trục khác theo lựa chọn hiện tại + Active.
                var target =
                    variants.FirstOrDefault(v => v.Active && ValueOf(v, name) == value && MatchesOtherAxes(v, selected, name, axisNames, ValueOf))
                    ?? variants.FirstOrDefault(v => v.Active && ValueOf(v, name) == value)
                    ?? variants.FirstOrDefault(v => ValueOf(v, name) == value);

                options.Add(new VariantAxisOptionDto(
                    Value: value,
                    Slug: target?.Slug,
                    IsSelected: ValueOf(selected, name) == value,
                    Available: target?.Active ?? false));
            }
            axes.Add(new VariantAxisDto(name, options));
        }
        return axes;
    }

    private static bool MatchesOtherAxes(
        VariantInfo candidate, VariantInfo selected, string currentAxis,
        IReadOnlyList<string> axisNames, Func<VariantInfo, string, string?> valueOf)
    {
        foreach (var other in axisNames)
        {
            if (other == currentAxis) continue;
            if (valueOf(candidate, other) != valueOf(selected, other)) return false;
        }
        return true;
    }
}
