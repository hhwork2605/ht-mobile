using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Bundles.Dtos;
using HtMobile.Application.Features.Cart;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Bundles;

/// <summary>
/// Combo "mua kèm phụ kiện" (P3-01). Đọc bundle cho PDP + thêm vào giỏ với GIÁ MUA KÈM lấy TỪ DB
/// (<see cref="Domain.Entities.Sales.BundleItem.BundlePrice"/>) — không tin giá client gửi lên.
/// </summary>
public class BundleService
{
    private readonly IApplicationDbContext _db;
    private readonly IPricingService _pricing;
    private readonly ICartService _cart;

    public BundleService(IApplicationDbContext db, IPricingService pricing, ICartService cart)
    {
        _db = db;
        _pricing = pricing;
        _cart = cart;
    }

    /// <summary>Khối bundle của 1 sản phẩm (null nếu không có combo).</summary>
    public async Task<BundleView?> GetForProductAsync(long productId, CancellationToken ct = default)
    {
        var bundle = await _db.Bundles
            .AsNoTracking()
            .Where(b => b.MainProductId == productId)
            .Select(b => b.Items
                .Where(i => i.AccessoryProduct.Status == Domain.Enums.ProductStatus.Active)
                .Select(i => new
            {
                i.AccessoryProductId,
                i.BundlePrice,
                // Tên hiển thị = model cha của phụ kiện (nếu có), ảnh gallery cũng ở model cha.
                ProductName = i.AccessoryProduct.Parent != null ? i.AccessoryProduct.Parent.Name : i.AccessoryProduct.Name,
                Slug = i.AccessoryProduct.Slug,
                Attrs = i.AccessoryProduct.Attributes.OrderBy(a => a.Attribute.SortOrder).ThenBy(a => a.AttributeId).Select(a => a.Value).ToList(),
                // Ảnh gallery ở model cha (biến thể con không có ảnh riêng). Parent null → null (an toàn, dịch được).
                Thumb = i.AccessoryProduct.Parent!.Images.OrderBy(im => im.SortOrder).Select(im => im.Url).FirstOrDefault()
            }).ToList())
            .FirstOrDefaultAsync(ct);

        if (bundle is null || bundle.Count == 0) return null;

        var accessories = new List<BundleAccessoryView>();
        foreach (var it in bundle)
        {
            var listed = (await _pricing.GetEffectivePriceAsync(it.AccessoryProductId, ct)).FinalPrice;
            var saving = listed - it.BundlePrice;
            if (saving < 0) saving = 0m;
            accessories.Add(new BundleAccessoryView
            {
                VariantId = it.AccessoryProductId,
                ProductName = it.ProductName,
                VariantText = string.Join(" · ", it.Attrs),
                ThumbnailUrl = it.Thumb,
                VariantSlug = it.Slug,
                ListedPrice = listed,
                BundlePrice = it.BundlePrice,
                Saving = saving
            });
        }

        var totals = BundleMath.Summarize(accessories.Select(a => new BundleLineInput(a.ListedPrice, a.BundlePrice)));
        return new BundleView
        {
            Accessories = accessories,
            TotalListed = totals.TotalListed,
            TotalBundle = totals.TotalBundle,
            TotalSaving = totals.TotalSaving
        };
    }

    /// <summary>Thêm SP chính (giá thường) + phụ kiện đã chọn (giá mua kèm từ DB) vào giỏ. Trả tổng số lượng giỏ.</summary>
    public async Task<int> AddToCartAsync(CartOwner owner, long mainVariantId, IEnumerable<long> accessoryVariantIds, CancellationToken ct = default)
    {
        await _cart.AddItemAsync(owner, mainVariantId, 1, null, ct);

        // Bundle gắn với model (cha). Biến thể chính là Product con → model = ProductParentId (hoặc chính nó).
        var mainModelId = await _db.Products
            .Where(p => p.Id == mainVariantId)
            .Select(p => (long?)(p.ProductParentId ?? p.Id))
            .FirstOrDefaultAsync(ct);
        if (mainModelId is null) return await _cart.GetCountAsync(owner, ct);

        var selected = accessoryVariantIds.Distinct().ToHashSet();
        if (selected.Count == 0) return await _cart.GetCountAsync(owner, ct);

        // Giá mua kèm LẤY TỪ DB, chỉ cho phụ kiện thực sự thuộc bundle của sản phẩm chính.
        var bundleItems = await _db.Bundles
            .Where(b => b.MainProductId == mainModelId)
            .SelectMany(b => b.Items)
            .Where(i => selected.Contains(i.AccessoryProductId))
            .Select(i => new { i.AccessoryProductId, i.BundlePrice })
            .ToListAsync(ct);

        foreach (var bi in bundleItems)
            await _cart.AddItemAsync(owner, bi.AccessoryProductId, 1, bi.BundlePrice, ct);

        return await _cart.GetCountAsync(owner, ct);
    }
}
