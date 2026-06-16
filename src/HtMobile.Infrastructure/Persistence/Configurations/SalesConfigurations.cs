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
        builder.HasIndex(i => new { i.CartId, i.VariantId }).IsUnique();
    }
}
