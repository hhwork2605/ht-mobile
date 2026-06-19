using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Catalog.Dtos;
using HtMobile.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Catalog;

/// <summary>
/// Quản lý ảnh sản phẩm cho Admin (gắn ở <b>model</b> = Product cha; storefront PDP đọc <c>ProductImages</c>
/// theo modelId). File lưu qua <see cref="IFileStorage"/>; <c>ProductImage.Url</c> lưu URL công khai.
/// </summary>
public class AdminProductImageService
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorage _storage;

    public AdminProductImageService(IApplicationDbContext db, IFileStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<IReadOnlyList<ProductImageRow>> ListAsync(long productId, CancellationToken ct = default)
        => await _db.ProductImages.AsNoTracking()
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.SortOrder).ThenBy(i => i.Id)
            .Select(i => new ProductImageRow(i.Id, i.Url, i.SortOrder))
            .ToListAsync(ct);

    /// <summary>Lưu file rồi thêm 1 ảnh vào cuối danh sách của model. <paramref name="publicBaseUrl"/> = scheme://host của API (ghép URL tuyệt đối).</summary>
    public async Task<(AdminProductResult Result, ProductImageRow? Image)> AddAsync(
        long productId, Stream content, string fileName, string publicBaseUrl, CancellationToken ct = default)
    {
        // Chỉ gắn ảnh vào model (Product cha). Kiểm tra trước khi ghi file để tránh file mồ côi.
        var isModel = await _db.Products.AnyAsync(p => p.Id == productId && p.ProductParentId == null, ct);
        if (!isModel) return (AdminProductResult.NotFound, null);

        var relative = await _storage.SaveAsync(content, fileName, "products", ct);
        var url = string.IsNullOrEmpty(publicBaseUrl) ? relative : publicBaseUrl.TrimEnd('/') + relative;

        var nextSort = await _db.ProductImages.Where(i => i.ProductId == productId)
            .Select(i => (int?)i.SortOrder).MaxAsync(ct) ?? -1;

        var image = new ProductImage { ProductId = productId, Url = url, SortOrder = nextSort + 1 };
        _db.ProductImages.Add(image);
        await _db.SaveChangesAsync(ct);

        return (AdminProductResult.Ok, new ProductImageRow(image.Id, image.Url, image.SortOrder));
    }

    public async Task<AdminProductResult> DeleteAsync(long imageId, CancellationToken ct = default)
    {
        var image = await _db.ProductImages.FirstOrDefaultAsync(i => i.Id == imageId, ct);
        if (image is null) return AdminProductResult.NotFound;

        await _storage.DeleteAsync(image.Url, ct);
        _db.ProductImages.Remove(image);
        await _db.SaveChangesAsync(ct);
        return AdminProductResult.Ok;
    }

    /// <summary>Đặt lại SortOrder theo thứ tự <paramref name="orderedIds"/> (chỉ ảnh thuộc model này).</summary>
    public async Task<AdminProductResult> ReorderAsync(long productId, IReadOnlyList<long> orderedIds, CancellationToken ct = default)
    {
        var images = await _db.ProductImages.Where(i => i.ProductId == productId).ToListAsync(ct);
        if (images.Count == 0) return AdminProductResult.NotFound;

        for (var i = 0; i < orderedIds.Count; i++)
        {
            var img = images.FirstOrDefault(x => x.Id == orderedIds[i]);
            if (img is not null) img.SortOrder = i;
        }
        await _db.SaveChangesAsync(ct);
        return AdminProductResult.Ok;
    }
}
