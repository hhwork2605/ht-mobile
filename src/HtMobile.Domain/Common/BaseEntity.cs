namespace HtMobile.Domain.Common;

/// <summary>Lớp cơ sở cho mọi entity. Khóa chính kiểu <see cref="long"/>.</summary>
public abstract class BaseEntity
{
    public long Id { get; set; }
}
