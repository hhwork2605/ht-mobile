namespace HtMobile.Application.Common.Interfaces;

/// <summary>Gợi ý tìm kiếm (autocomplete). Hiện thực mặc định: Postgres FTS.</summary>
public interface ISearchService
{
    Task<SearchSuggestResult> SuggestAsync(string query, int limit = 8, CancellationToken ct = default);
}

public record SearchSuggestResult(
    IReadOnlyList<string> Keywords,
    IReadOnlyList<ProductSuggestion> Products);

public record ProductSuggestion(
    long VariantId,
    string Name,
    string Slug,
    string? ThumbnailUrl,
    decimal Price,
    decimal? CompareAtPrice);
