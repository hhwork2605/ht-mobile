using HtMobile.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Infrastructure.Search;

/// <summary>
/// Gợi ý tìm kiếm bằng PostgreSQL (ILIKE/pg_trgm) — đủ cho autocomplete giai đoạn đầu.
/// Nâng cấp tsvector/ranking hoặc đổi Meilisearch sau bằng cách thay class này (giữ nguyên <see cref="ISearchService"/>).
/// </summary>
public class PostgresSearchService : ISearchService
{
    private readonly IApplicationDbContext _db;

    public PostgresSearchService(IApplicationDbContext db) => _db = db;

    public async Task<SearchSuggestResult> SuggestAsync(string query, int limit = 8, CancellationToken ct = default)
    {
        query = query?.Trim() ?? string.Empty;
        if (query.Length < 2)
            return new SearchSuggestResult(Array.Empty<string>(), Array.Empty<ProductSuggestion>());

        var pattern = $"%{query}%";

        // Gợi ý biến thể (Product con) khớp tên model cha hoặc slug. Tên/ảnh lấy từ model cha.
        var rows = await _db.Products
            .AsNoTracking()
            .Where(v => v.ProductParentId != null
                && (EF.Functions.ILike(v.Parent!.Name, pattern) || EF.Functions.ILike(v.Slug, pattern)))
            .OrderBy(v => v.Id)
            .Take(limit)
            .Select(v => new ProductSuggestion(
                v.Id,
                v.Parent!.Name,
                v.Slug,
                v.Parent.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
                v.BasePrice,
                v.CompareAtPrice))
            .ToListAsync(ct);

        var keywords = rows.Select(r => r.Name).Distinct().Take(limit).ToList();
        return new SearchSuggestResult(keywords, rows);
    }
}
