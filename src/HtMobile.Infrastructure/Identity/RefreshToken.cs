using HtMobile.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HtMobile.Infrastructure.Identity;

/// <summary>
/// 1 phiên đăng nhập admin (đa phiên). Mỗi lần login tạo 1 dòng; refresh xoay vòng (revoke dòng cũ + tạo dòng mới).
/// Chỉ lưu HASH của phần ngẫu nhiên (không lưu token thô). Refresh token client = "{UserId}.{random}".
/// </summary>
public class RefreshToken
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        b.HasIndex(x => x.TokenHash).IsUnique();
        b.HasIndex(x => x.UserId);
        b.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
