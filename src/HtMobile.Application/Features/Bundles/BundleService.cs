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
                .Where(i => i.AccessoryVariant.Status == Domain.Enums.VariantStatus.Active)
                .Select(i => new
            {
                i.AccessoryVariantId,
                i.BundlePrice,
                ProductName = i.AccessoryVariant.Product.Name,
                i.AccessoryVariant.Storage,
                i.AccessoryVariant.Color,
                Slug = i.AccessoryVariant.Slug,
                Thumb = i.AccessoryVariant.Product.Images.OrderBy(im => im.SortOrder).Select(im => im.Url).FirstOrDefault()
            }).ToList())
            .FirstOrDefaultAsync(ct);

        if (bundle is null || bundle.Count == 0) return null;

        var accessories = new List<BundleAccessoryView>();
        foreach (var it in bundle)
        {
            var listed = (await _pricing.GetEffectivePriceAsync(it.AccessoryVariantId, ct)).FinalPrice;
            var saving = listed - it.BundlePrice;
            if (saving < 0) saving = 0m;
            accessories.Add(new BundleAccessoryView
            {
                VariantId = it.AccessoryVariantId,
                ProductName = it.ProductName,
                VariantText = string.Join(" · ", new[] { it.Color, it.Storage }.Where(s => !string.IsNullOrWhiteSpace(s))),
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

        var mainProductId = await _db.ProductVariants
            .Where(v => v.Id == mainVariantId)
            .Select(v => (long?)v.ProductId)
            .FirstOrDefaultAsync(ct);
        if (mainProductId is null) return await _cart.GetCountAsync(owner, ct);

        var selected = accessoryVariantIds.Distinct().ToHashSet();
        if (selected.Count == 0) return await _cart.GetCountAsync(owner, ct);

        // Giá mua kèm LẤY TỪ DB, chỉ cho phụ kiện thực sự thuộc bundle của sản phẩm chính.
        var bundleItems = await _db.Bundles
            .Where(b => b.MainProductId == mainProductId)
            .SelectMany(b => b.Items)
            .Where(i => selected.Contains(i.AccessoryVariantId))
            .Select(i => new { i.AccessoryVariantId, i.BundlePrice })
            .ToListAsync(ct);

        foreach (var bi in bundleItems)
            await _cart.AddItemAsync(owner, bi.AccessoryVariantId, 1, bi.BundlePrice, ct);

        return await _cart.GetCountAsync(owner, ct);
    }
}
