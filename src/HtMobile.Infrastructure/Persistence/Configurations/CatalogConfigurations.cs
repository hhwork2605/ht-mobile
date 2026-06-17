using HtMobile.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AttributeEntity = HtMobile.Domain.Entities.Catalog.Attribute;

namespace HtMobile.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();

        b.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.Property(x => x.Name).HasMaxLength(300).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(300).IsRequired();
        b.Property(x => x.Sku).HasMaxLength(80);
        b.Property(x => x.Tagline).HasMaxLength(200);
        b.Property(x => x.Brand).HasMaxLength(100);
        b.Property(x => x.SpecsJson).HasColumnType("jsonb");
        b.HasIndex(x => x.Slug).IsUnique();
        // SKU chỉ đặt cho biến thể con → unique nhưng bỏ qua NULL (cha không có SKU).
        b.HasIndex(x => x.Sku).IsUnique().HasFilter("\"Sku\" IS NOT NULL");

        b.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tự tham chiếu cha–con (biến thể). Xoá cha không tự xoá con (Restrict) — xử lý ở nghiệp vụ.
        b.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ProductParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AttributeConfiguration : IEntityTypeConfiguration<AttributeEntity>
{
    public void Configure(EntityTypeBuilder<AttributeEntity> b)
    {
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
    }
}

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> b)
    {
        b.Property(x => x.Value).HasMaxLength(500).IsRequired();
        b.HasIndex(x => new { x.ProductId, x.AttributeId }).IsUnique();

        b.HasOne(x => x.Product)
            .WithMany(x => x.Attributes)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Attribute)
            .WithMany(x => x.ProductAttributes)
            .HasForeignKey(x => x.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> b)
    {
        b.Property(x => x.Url).HasMaxLength(500).IsRequired();
        b.HasOne(x => x.Product).WithMany(x => x.Images).HasForeignKey(x => x.ProductId);
    }
}
