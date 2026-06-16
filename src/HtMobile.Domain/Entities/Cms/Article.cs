using HtMobile.Domain.Common;

namespace HtMobile.Domain.Entities.Cms;

/// <summary>Bài viết tin tức/blog (newsfeed). SPEC §3, §7.</summary>
public class Article : BaseAuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Body { get; set; }
    public DateTime? PublishedAt { get; set; }
}
