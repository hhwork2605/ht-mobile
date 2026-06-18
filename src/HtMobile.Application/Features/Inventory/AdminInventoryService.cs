using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Inventory.Dtos;
using Microsoft.EntityFrameworkCore;
using InventoryEntity = HtMobile.Domain.Entities.Inventory.Inventory;
using StoreEntity = HtMobile.Domain.Entities.Inventory.Store;

namespace HtMobile.Application.Features.Inventory;

/// <summary>Quản lý cửa hàng + tồn kho theo cửa hàng (admin). Tồn kho gắn với biến thể = Product con.</summary>
public class AdminInventoryService
{
    private readonly IApplicationDbContext _db;

    public AdminInventoryService(IApplicationDbContext db) => _db = db;

    // ===== Cửa hàng =====

    public async Task<IReadOnlyList<StoreRow>> GetStoresAsync(CancellationToken ct = default)
        => await _db.Stores
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new StoreRow { Id = s.Id, Name = s.Name, Address = s.Address, Phone = s.Phone })
            .ToListAsync(ct);

    public async Task<long> CreateStoreAsync(StoreInput input, CancellationToken ct = default)
    {
        var store = new StoreEntity { Name = input.Name.Trim(), Address = Trim(input.Address), Phone = Trim(input.Phone) };
        _db.Stores.Add(store);
        await _db.SaveChangesAsync(ct);
        return store.Id;
    }

    public async Task<AdminInventoryResult> UpdateStoreAsync(long id, StoreInput input, CancellationToken ct = default)
    {
        var store = await _db.Stores.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (store is null) return AdminInventoryResult.NotFound;
        store.Name = input.Name.Trim();
        store.Address = Trim(input.Address);
        store.Phone = Trim(input.Phone);
        await _db.SaveChangesAsync(ct);
        return AdminInventoryResult.Ok;
    }

    public async Task<AdminInventoryResult> DeleteStoreAsync(long id, CancellationToken ct = default)
    {
        var store = await _db.Stores.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (store is null) return AdminInventoryResult.NotFound;
        if (await _db.Inventories.AnyAsync(i => i.StoreId == id && i.Quantity > 0, ct)) return AdminInventoryResult.HasStock;

        // Xoá cả dòng tồn = 0 của cửa hàng rồi xoá cửa hàng.
        var zero = await _db.Inventories.Where(i => i.StoreId == id).ToListAsync(ct);
        if (zero.Count > 0) _db.Inventories.RemoveRange(zero);
        _db.Stores.Remove(store);
        await _db.SaveChangesAsync(ct);
        return AdminInventoryResult.Ok;
    }

    // ===== Tồn kho theo cửa hàng =====

    /// <summary>Tồn kho mọi biến thể (Product con) tại 1 cửa hàng — chưa có dòng tồn thì hiển thị 0.</summary>
    public async Task<IReadOnlyList<StockRow>> GetStockAsync(long storeId, CancellationToken ct = default)
    {
        var variants = await _db.Products
            .AsNoTracking()
            .Where(p => p.ProductParentId != null)
            .OrderBy(p => p.Parent!.Name).ThenBy(p => p.Id)
            .Select(p => new
            {
                p.Id,
                ProductName = p.Parent!.Name,
                p.Sku,
                Attrs = p.Attributes.OrderBy(a => a.Attribute.SortOrder).ThenBy(a => a.AttributeId).Select(a => a.Value).ToList(),
            })
            .ToListAsync(ct);

        var qty = await _db.Inventories
            .AsNoTracking()
            .Where(i => i.StoreId == storeId)
            .ToDictionaryAsync(i => i.ProductId, i => i.Quantity, ct);

        return variants.Select(v => new StockRow
        {
            ProductId = v.Id,
            ProductName = v.ProductName,
            VariantLabel = string.Join(" · ", v.Attrs),
            Sku = v.Sku,
            Quantity = qty.GetValueOrDefault(v.Id),
        }).ToList();
    }

    /// <summary>Upsert tồn kho cho 1 cửa hàng (chỉ biến thể con hợp lệ).</summary>
    public async Task<AdminInventoryResult> SetStockAsync(long storeId, IReadOnlyList<StockUpdateItem> items, CancellationToken ct = default)
    {
        if (!await _db.Stores.AnyAsync(s => s.Id == storeId, ct)) return AdminInventoryResult.NotFound;

        var ids = items.Select(i => i.ProductId).ToList();
        var validIds = (await _db.Products.Where(p => ids.Contains(p.Id) && p.ProductParentId != null)
            .Select(p => p.Id).ToListAsync(ct)).ToHashSet();
        var existing = await _db.Inventories.Where(i => i.StoreId == storeId && ids.Contains(i.ProductId)).ToListAsync(ct);
        var byProduct = existing.ToDictionary(i => i.ProductId);

        foreach (var item in items)
        {
            if (!validIds.Contains(item.ProductId)) continue;
            var q = item.Quantity < 0 ? 0 : item.Quantity;
            if (byProduct.TryGetValue(item.ProductId, out var inv))
                inv.Quantity = q;
            else
                _db.Inventories.Add(new InventoryEntity { StoreId = storeId, ProductId = item.ProductId, Quantity = q });
        }
        await _db.SaveChangesAsync(ct);
        return AdminInventoryResult.Ok;
    }

    private static string? Trim(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
