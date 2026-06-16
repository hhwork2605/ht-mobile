using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace HtMobile.Web.Infrastructure.Seo;

/// <summary>
/// Helper render JSON-LD schema.org cho SEO (xem docs/conventions.md §SEO).
/// Dùng <c>Dictionary&lt;string, object?&gt;</c> với khóa <c>"@type"/"@context"</c> để né việc Razor
/// nuốt ký tự <c>@</c>, và encoder relaxed để không escape Unicode (tiếng Việt) trong &lt;script&gt;.
/// </summary>
public static class JsonLd
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>Serialize 1 object JSON-LD ra chuỗi để nhét vào <c>&lt;script type="application/ld+json"&gt;</c>.</summary>
    public static string Serialize(object graph) => JsonSerializer.Serialize(graph, Options);

    /// <summary>Dựng <c>BreadcrumbList</c> từ các cặp (tên, URL tuyệt đối) theo thứ tự.</summary>
    public static string Breadcrumb(IEnumerable<(string Name, string Url)> items)
    {
        var position = 1;
        var elements = items.Select(it => new Dictionary<string, object?>
        {
            ["@type"] = "ListItem",
            ["position"] = position++,
            ["name"] = it.Name,
            ["item"] = it.Url
        }).ToList();

        return Serialize(new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "BreadcrumbList",
            ["itemListElement"] = elements
        });
    }
}
