using HtMobile.Domain.Entities.Customers;
using HtMobile.Domain.Entities.Reviews;
using HtMobile.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HtMobile.Infrastructure.Persistence.Configurations;

/// <summary>Khách hàng storefront: email UNIQUE (chuẩn hoá thường); tách khỏi Identity (ADR 0004).</summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(c => c.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(c => c.Email).IsUnique();
        builder.Property(c => c.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(c => c.FullName).HasMaxLength(200);
        builder.Property(c => c.Phone).HasMaxLength(30);
        builder.Property(c => c.ResetTokenHash).HasMaxLength(128);
    }
}

/// <summary>Đơn hàng gắn Customer (FK; null = guest). Xoá Customer bị chặn nếu còn đơn (giữ lịch sử).</summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => o.CustomerId);
    }
}

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
