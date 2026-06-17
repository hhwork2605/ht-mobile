using HtMobile.Domain.Entities.Reviews;
using HtMobile.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HtMobile.Infrastructure.Persistence.Configurations;

/// <summary>Giỏ hàng: mỗi user 1 giỏ, mỗi phiên guest 1 giỏ (unique index lọc null).</summary>
public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasIndex(c => c.CustomerId)
            .IsUnique()
            .HasFilter("\"CustomerId\" IS NOT NULL");

        builder.HasIndex(c => c.SessionId)
            .IsUnique()
            .HasFilter("\"SessionId\" IS NOT NULL");
    }
}

/// <summary>Dòng giỏ: 1 biến thể chỉ xuất hiện 1 lần trong 1 giỏ (chống trùng dòng khi race/merge).</summary>
public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasIndex(i => new { i.CartId, i.ProductId }).IsUnique();
        // Xoá Product KHÔNG cascade xoá dòng giỏ (giữ toàn vẹn; xử lý ở nghiệp vụ).
        builder.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>Dòng đơn = lịch sử: xoá Product KHÔNG được cascade xoá lịch sử đơn (ADR 0003, review C2).</summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>Đánh giá: xoá Product KHÔNG cascade xoá review.</summary>
public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasOne(r => r.Product).WithMany().HasForeignKey(r => r.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>Phụ kiện bundle: xoá Product KHÔNG cascade xoá cấu hình bundle.</summary>
public class BundleItemConfiguration : IEntityTypeConfiguration<BundleItem>
{
    public void Configure(EntityTypeBuilder<BundleItem> builder)
    {
        builder.HasOne(i => i.AccessoryProduct).WithMany().HasForeignKey(i => i.AccessoryProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
