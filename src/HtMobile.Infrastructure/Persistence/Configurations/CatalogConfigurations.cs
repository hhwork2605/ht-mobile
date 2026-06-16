using HtMobile.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
        b.Property(x => x.Tagline).HasMaxLength(200);
        b.Property(x => x.Brand).HasMaxLength(100);
        b.Property(x => x.SpecsJson).HasColumnType("jsonb");
        b.HasIndex(x => x.Slug).IsUnique();

        b.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> b)
    {
        b.Property(x => x.Sku).HasMaxLength(80).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(320).IsRequired();
        b.Property(x => x.Storage).HasMaxLength(40);
        b.Property(x => x.Color).HasMaxLength(60);
        b.HasIndex(x => x.Slug).IsUnique();
        b.HasIndex(x => x.Sku).IsUnique();

        b.HasOne(x => x.Product)
            .WithMany(x => x.Variants)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> b)
    {
        b.Property(x => x.Url).HasMaxLength(500).IsRequired();
        b.HasOne(x => x.Product).WithMany(x => x.Images).HasForeignKey(x => x.ProductId);
        b.HasOne(x => x.Variant).WithMany(x => x.Images).HasForeignKey(x => x.VariantId).OnDelete(DeleteBehavior.SetNull);
    }
}
