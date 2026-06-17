using System.ComponentModel.DataAnnotations;
using HtMobile.Application.Features.Catalog.Dtos;
using HtMobile.Domain.Enums;

namespace HtMobile.Web.Areas.Admin.Models;

/// <summary>Giá gạch (giá gốc) không được nhỏ hơn giá bán — tránh "giảm giá âm" ở storefront.</summary>
internal static class PriceRule
{
    public static IEnumerable<ValidationResult> Check(decimal basePrice, decimal? compareAt, string memberName)
    {
        if (compareAt is not null && compareAt < basePrice)
            yield return new ValidationResult("Giá gạch phải ≥ giá bán.", new[] { memberName });
    }
}

/// <summary>Form tạo SP: field SP + 1 biến thể đầu.</summary>
public class ProductCreateVm : IValidatableObject
{
    [Required(ErrorMessage = "Nhập tên sản phẩm.")]
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    [Required(ErrorMessage = "Chọn danh mục.")]
    public long CategoryId { get; set; }
    public string? Brand { get; set; }
    public string? Tagline { get; set; }
    public string? Description { get; set; }

    [Required(ErrorMessage = "Nhập SKU biến thể.")]
    public string Sku { get; set; } = string.Empty;
    public string? Storage { get; set; }
    public string? Color { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Giá phải ≥ 0.")]
    public decimal BasePrice { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Giá gạch phải ≥ 0.")]
    public decimal? CompareAtPrice { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Active;

    public IReadOnlyList<AdminCategoryOption> Categories { get; set; } = new List<AdminCategoryOption>();

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
        => PriceRule.Check(BasePrice, CompareAtPrice, nameof(CompareAtPrice));
}

/// <summary>Form sửa SP: field SP + sửa giá/trạng thái từng biến thể.</summary>
public class ProductEditVm
{
    public long Id { get; set; }
    [Required(ErrorMessage = "Nhập tên sản phẩm.")]
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    [Required(ErrorMessage = "Chọn danh mục.")]
    public long CategoryId { get; set; }
    public string? Brand { get; set; }
    public string? Tagline { get; set; }
    public string? Description { get; set; }

    public List<VariantEditVm> Variants { get; set; } = new();
    public IReadOnlyList<AdminCategoryOption> Categories { get; set; } = new List<AdminCategoryOption>();
}

public class VariantEditVm : IValidatableObject
{
    public long Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Storage { get; set; }
    public string? Color { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Giá phải ≥ 0.")]
    public decimal BasePrice { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Giá gạch phải ≥ 0.")]
    public decimal? CompareAtPrice { get; set; }
    public ProductStatus Status { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
        => PriceRule.Check(BasePrice, CompareAtPrice, nameof(CompareAtPrice));
}

/// <summary>Form thêm biến thể (AJAX) trên màn sửa SP. ProductId lấy từ route, không cần ở đây.</summary>
public class VariantAddVm : IValidatableObject
{
    [Required(ErrorMessage = "Nhập SKU.")]
    public string Sku { get; set; } = string.Empty;
    public string? Storage { get; set; }
    public string? Color { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Giá phải ≥ 0.")]
    public decimal BasePrice { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Giá gạch phải ≥ 0.")]
    public decimal? CompareAtPrice { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Active;

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
        => PriceRule.Check(BasePrice, CompareAtPrice, nameof(CompareAtPrice));
}
