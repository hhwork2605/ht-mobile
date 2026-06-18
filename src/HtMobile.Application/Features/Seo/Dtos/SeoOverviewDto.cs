namespace HtMobile.Application.Features.Seo.Dtos;

/// <summary>1 mục có vấn đề SEO (sản phẩm/danh mục) — kèm link sửa ở FE.</summary>
public record SeoIssueItem(string Type, long Id, string Name);

/// <summary>1 nhóm cảnh báo SEO.</summary>
public record SeoIssueGroup
{
    public string Key { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Severity { get; init; } = "warn";   // warn | danger | info
    public int Count { get; init; }
    public IReadOnlyList<SeoIssueItem> Items { get; init; } = Array.Empty<SeoIssueItem>();
}

/// <summary>Tổng quan & kiểm tra SEO catalog.</summary>
public record SeoOverviewDto
{
    public int SitemapUrlCount { get; init; }
    public int ProductModelCount { get; init; }
    public int CategoryCount { get; init; }
    public int TotalIssues { get; init; }
    public IReadOnlyList<SeoIssueGroup> Groups { get; init; } = Array.Empty<SeoIssueGroup>();
}
