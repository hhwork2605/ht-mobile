using System.Text.Json;
using HtMobile.Application.Features.Catalog.Dtos;

namespace HtMobile.Application.Features.Catalog;

/// <summary>
/// Parse/validate thông số kỹ thuật lưu ở <c>Product.SpecsJson</c> (jsonb).
/// <para>
/// <b>Contract chuẩn</b> = mảng nhóm:
/// <code>[ { "group": "Tổng quan", "items": [ { "label": "Màn hình", "value": "6.9\"" } ] } ]</code>
/// Ngoài ra chấp nhận (tương thích ngược / tiện seed):
/// mảng phẳng <c>[ { "label", "value" } ]</c>, mảng cặp <c>[ ["Màn hình","6.9\""] ]</c>,
/// và object <c>{ "Màn hình": "6.9\"" }</c> — đều quy về 1 nhóm không tiêu đề.
/// </para>
/// JSON không hợp lệ / rỗng → danh sách rỗng (PDP tự ẩn mục thông số).
/// </summary>
public static class ProductSpecs
{
    public static IReadOnlyList<SpecGroupDto> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return Array.Empty<SpecGroupDto>();

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return root.ValueKind switch
            {
                JsonValueKind.Array => ParseArray(root),
                JsonValueKind.Object => Wrap(ParseObject(root)),
                _ => Array.Empty<SpecGroupDto>(),
            };
        }
        catch (JsonException)
        {
            return Array.Empty<SpecGroupDto>();
        }
    }

    /// <summary>true nếu chuỗi là JSON hợp lệ cho specs (rỗng/null cũng coi là hợp lệ = "không có thông số").</summary>
    public static bool IsValid(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return true;
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind is JsonValueKind.Array or JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static IReadOnlyList<SpecGroupDto> ParseArray(JsonElement arr)
    {
        // Mảng nhóm: phần tử object có "items" → mỗi phần tử là 1 nhóm.
        var isGroupedArray = arr.EnumerateArray()
            .Any(e => e.ValueKind == JsonValueKind.Object && e.TryGetProperty("items", out _));

        if (isGroupedArray)
        {
            var groups = new List<SpecGroupDto>();
            foreach (var g in arr.EnumerateArray())
            {
                if (g.ValueKind != JsonValueKind.Object) continue;
                var items = g.TryGetProperty("items", out var itemsEl) && itemsEl.ValueKind == JsonValueKind.Array
                    ? ParseItems(itemsEl)
                    : Array.Empty<SpecItemDto>();
                if (items.Count == 0) continue;
                groups.Add(new SpecGroupDto(Str(g, "group") ?? Str(g, "name"), items));
            }
            return groups;
        }

        // Mảng phẳng (item objects / cặp [k,v]) → 1 nhóm không tiêu đề.
        return Wrap(ParseItems(arr));
    }

    private static IReadOnlyList<SpecItemDto> ParseItems(JsonElement arr)
    {
        var items = new List<SpecItemDto>();
        foreach (var el in arr.EnumerateArray())
        {
            if (el.ValueKind == JsonValueKind.Object)
            {
                var label = Str(el, "label") ?? Str(el, "k") ?? Str(el, "name") ?? Str(el, "key");
                var value = Str(el, "value") ?? Str(el, "v") ?? Str(el, "val");
                if (!string.IsNullOrWhiteSpace(label))
                    items.Add(new SpecItemDto(label!.Trim(), (value ?? string.Empty).Trim()));
            }
            else if (el.ValueKind == JsonValueKind.Array && el.GetArrayLength() >= 2)
            {
                var label = el[0].ToString();
                if (!string.IsNullOrWhiteSpace(label))
                    items.Add(new SpecItemDto(label.Trim(), el[1].ToString().Trim()));
            }
        }
        return items;
    }

    private static IReadOnlyList<SpecItemDto> ParseObject(JsonElement obj)
    {
        var items = new List<SpecItemDto>();
        foreach (var p in obj.EnumerateObject())
            items.Add(new SpecItemDto(p.Name, p.Value.ToString()));
        return items;
    }

    private static IReadOnlyList<SpecGroupDto> Wrap(IReadOnlyList<SpecItemDto> items)
        => items.Count == 0 ? Array.Empty<SpecGroupDto>() : new[] { new SpecGroupDto(null, items) };

    private static string? Str(JsonElement obj, string prop)
        => obj.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
}
