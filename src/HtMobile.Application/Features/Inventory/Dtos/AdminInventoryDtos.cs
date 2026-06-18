namespace HtMobile.Application.Features.Inventory.Dtos;

/// <summary>1 cửa hàng (admin).</summary>
public record StoreRow
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Address { get; init; }
    public string? Phone { get; init; }
}

/// <summary>Input tạo/sửa cửa hàng.</summary>
public record StoreInput(string Name, string? Address, string? Phone);

/// <summary>Tồn kho 1 biến thể (Product con) tại 1 cửa hàng.</summary>
public record StockRow
{
    public long ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string VariantLabel { get; init; } = string.Empty;
    public string? Sku { get; init; }
    public int Quantity { get; init; }
}

/// <summary>1 dòng cập nhật tồn kho.</summary>
public record StockUpdateItem(long ProductId, int Quantity);

/// <summary>Kết quả thao tác kho/cửa hàng.</summary>
public enum AdminInventoryResult { Ok, NotFound, HasStock }
