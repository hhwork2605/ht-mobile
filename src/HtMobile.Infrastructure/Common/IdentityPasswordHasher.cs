using HtMobile.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace HtMobile.Infrastructure.Common;

/// <summary>
/// Hiện thực <see cref="IPasswordHasher"/> bằng <see cref="PasswordHasher{T}"/> của ASP.NET Core
/// (PBKDF2, định dạng PHC v3). Dùng cho tài khoản Customer storefront — không kéo theo Identity store.
/// </summary>
public class IdentityPasswordHasher : IPasswordHasher
{
    private static readonly object Subject = new();
    private readonly PasswordHasher<object> _inner = new();

    public string Hash(string password) => _inner.HashPassword(Subject, password);

    public bool Verify(string hash, string password)
        => _inner.VerifyHashedPassword(Subject, hash, password) != PasswordVerificationResult.Failed;
}
