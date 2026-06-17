using HtMobile.Domain.Enums;

namespace HtMobile.Application.Features.Catalog.Dtos;

/// <summary>1 dòng trong bảng SP admin: tên + danh mục + SKU đại diện + số biến thể + khoảng giá + còn bán?</summary>
public class AdminProductRow
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public int VariantCount { get; set; }
    public decimal PriceFrom { get; set; }
    public decimal PriceTo { get; set; }
    /// <summary>Có ít nhất 1 biến thể Active.</summary>
    public bool IsActive { get; set; }
}

/// <summary>Tuỳ chọn danh mục cho dropdown lọc/tạo SP.</summary>
public record AdminCategoryOption(long Id, string Name);

/// <summary>Trang danh sách SP admin: rows + danh mục + bộ lọc hiện tại.</summary>
public class AdminProductListDto
{
    public IReadOnlyList<AdminProductRow> Products { get; set; } = new List<AdminProductRow>();
    public IReadOnlyList<AdminCategoryOption> Categories { get; set; } = new List<AdminCategoryOption>();
    public long? CategoryId { get; set; }
}

/// <summary>1 biến thể khi sửa SP.</summary>
public class AdminVariantRow
{
    public long Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Storage { get; set; }
    public string? Color { get; set; }
    public decimal BasePrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public ProductStatus Status { get; set; }
}

/// <summary>Dữ liệu cho màn sửa SP: field SP + danh sách biến thể + danh mục.</summary>
public class AdminProductEditDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public long CategoryId { get; set; }
    public string? Brand { get; set; }
    public string? Tagline { get; set; }
    public string? Description { get; set; }
    public IReadOnlyList<AdminVariantRow> Variants { get; set; } = new List<AdminVariantRow>();
    public IReadOnlyList<AdminCategoryOption> Categories { get; set; } = new List<AdminCategoryOption>();
}

/// <summary>Input field SP (tạo/sửa). Slug để trống → tự sinh từ Name.</summary>
public record ProductInput(string Name, string? Slug, long CategoryId, string? Brand, string? Tagline, string? Description);

/// <summary>Input 1 biến thể mới.</summary>
public record VariantInput(string Sku, string? Storage, string? Color, decimal BasePrice, decimal? CompareAtPrice, ProductStatus Status);

/// <summary>Sửa 1 biến thể có sẵn (chỉ giá + trạng thái).</summary>
public record VariantEdit(long Id, decimal BasePrice, decimal? CompareAtPrice, ProductStatus Status);

/// <summary>Kết quả thao tác ghi SP.</summary>
public enum AdminProductResult { Ok, SlugExists, SkuExists, NotFound }
