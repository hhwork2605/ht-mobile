using HtMobile.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Web.Infrastructure.Routing;

public enum SlugKind { NotFound, Category, Variant, Article, Page }

public record SlugMatch(SlugKind Kind, string Slug);

/// <summary>Phân giải 1 slug ở gốc URL về loại entity (cho URL SEO phẳng /iphone, /dien-thoai-...).</summary>
public interface ISlugResolver
{
    Task<SlugMatch> ResolveAsync(string slug, CancellationToken ct = default);
}

public class SlugResolver : ISlugResolver
{
    private readonly IApplicationDbContext _db;

    public SlugResolver(IApplicationDbContext db) => _db = db;

    public async Task<SlugMatch> ResolveAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return new SlugMatch(SlugKind.NotFound, slug);

        if (await _db.Categories.AnyAsync(c => c.Slug == slug, ct))
            return new SlugMatch(SlugKind.Category, slug);

        // Biến thể = Product con; cũng nhận slug model cha → đều mở PDP (SlugKind.Variant).
        if (await _db.Products.AnyAsync(p => p.Slug == slug, ct))
            return new SlugMatch(SlugKind.Variant, slug);

        if (await _db.Articles.AnyAsync(a => a.Slug == slug, ct))
            return new SlugMatch(SlugKind.Article, slug);

        if (await _db.Pages.AnyAsync(p => p.Slug == slug, ct))
            return new SlugMatch(SlugKind.Page, slug);

        return new SlugMatch(SlugKind.NotFound, slug);
    }
}
