using HtMobile.Application.Common;
using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Catalog.Dtos;
using HtMobile.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Catalog;

/// <summary>CRUD danh mục cho admin. Slug auto-gen + unique; chống chu trình cha–con; chặn xoá khi còn con/sản phẩm.</summary>
public class AdminCategoryService
{
    private readonly IApplicationDbContext _db;

    public AdminCategoryService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<AdminCategoryRow>> GetListAsync(CancellationToken ct = default)
        => await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new AdminCategoryRow
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ParentName = c.Parent != null ? c.Parent.Name : null,
                SortOrder = c.SortOrder,
                ProductCount = c.Products.Count,
                ChildCount = c.Children.Count,
            })
            .ToListAsync(ct);

    public async Task<AdminCategoryDetail?> GetAsync(long id, CancellationToken ct = default)
        => await _db.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new AdminCategoryDetail
            {
                Id = c.Id, Name = c.Name, Slug = c.Slug, ParentId = c.ParentId,
                SortOrder = c.SortOrder, SeoContent = c.SeoContent,
            })
            .FirstOrDefaultAsync(ct);

    public async Task<(AdminCategoryResult Result, long Id)> CreateAsync(CategoryInput input, CancellationToken ct = default)
    {
        var slug = ResolveSlug(input.Slug, input.Name);
        if (await _db.Categories.AnyAsync(c => c.Slug == slug, ct))
            return (AdminCategoryResult.SlugExists, 0);
        if (input.ParentId is { } pid && !await _db.Categories.AnyAsync(c => c.Id == pid, ct))
            return (AdminCategoryResult.InvalidParent, 0);

        var cat = new Category
        {
            Name = input.Name.Trim(),
            Slug = slug,
            ParentId = input.ParentId,
            SortOrder = input.SortOrder,
            SeoContent = Trim(input.SeoContent),
        };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync(ct);
        return (AdminCategoryResult.Ok, cat.Id);
    }

    public async Task<AdminCategoryResult> UpdateAsync(long id, CategoryInput input, CancellationToken ct = default)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (cat is null) return AdminCategoryResult.NotFound;

        var slug = ResolveSlug(input.Slug, input.Name);
        if (slug != cat.Slug && await _db.Categories.AnyAsync(c => c.Slug == slug && c.Id != id, ct))
            return AdminCategoryResult.SlugExists;

        if (input.ParentId is { } pid)
        {
            if (pid == id) return AdminCategoryResult.InvalidParent;
            if (!await _db.Categories.AnyAsync(c => c.Id == pid, ct)) return AdminCategoryResult.InvalidParent;
            if (await IsDescendantAsync(ancestorId: id, candidateId: pid, ct)) return AdminCategoryResult.InvalidParent;
        }

        cat.Name = input.Name.Trim();
        cat.Slug = slug;
        cat.ParentId = input.ParentId;
        cat.SortOrder = input.SortOrder;
        cat.SeoContent = Trim(input.SeoContent);
        await _db.SaveChangesAsync(ct);
        return AdminCategoryResult.Ok;
    }

    public async Task<AdminCategoryResult> DeleteAsync(long id, CancellationToken ct = default)
    {
        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (cat is null) return AdminCategoryResult.NotFound;
        if (await _db.Categories.AnyAsync(c => c.ParentId == id, ct)) return AdminCategoryResult.HasChildren;
        if (await _db.Products.AnyAsync(p => p.CategoryId == id, ct)) return AdminCategoryResult.HasProducts;

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync(ct);
        return AdminCategoryResult.Ok;
    }

    /// <summary>candidate có nằm trong nhánh con của ancestor không? (chống đặt cha = chính con/cháu mình → chu trình).</summary>
    private async Task<bool> IsDescendantAsync(long ancestorId, long candidateId, CancellationToken ct)
    {
        var pairs = await _db.Categories.AsNoTracking().Select(c => new { c.Id, c.ParentId }).ToListAsync(ct);
        var parentOf = pairs.ToDictionary(p => p.Id, p => p.ParentId);
        long? cur = candidateId;
        var guard = 0;
        while (cur is { } id && guard++ < 1000)
        {
            if (id == ancestorId) return true;
            cur = parentOf.TryGetValue(id, out var p) ? p : null;
        }
        return false;
    }

    private static string ResolveSlug(string? slug, string name)
        => string.IsNullOrWhiteSpace(slug) ? Slugify.ToSlug(name) : Slugify.ToSlug(slug);

    private static string? Trim(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
