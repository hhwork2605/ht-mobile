using System.Security.Cryptography;
using System.Text;
using HtMobile.Application.Common.Interfaces;
using HtMobile.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Application.Features.Customers;

/// <summary>Kết quả đăng ký tài khoản Customer storefront.</summary>
public enum RegisterResult { Ok, EmailExists }

/// <summary>
/// Nghiệp vụ tài khoản Customer (storefront): đăng ký, kiểm tra đăng nhập, đặt lại mật khẩu.
/// Không phụ thuộc ASP.NET Identity — băm mật khẩu qua <see cref="IPasswordHasher"/>. Sign-in cookie xử lý ở Web.
/// </summary>
public class CustomerAccountService
{
    /// <summary>Token đặt lại mật khẩu hiệu lực trong 1 giờ.</summary>
    private static readonly TimeSpan ResetTokenTtl = TimeSpan.FromHours(1);

    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IDateTime _clock;

    public CustomerAccountService(IApplicationDbContext db, IPasswordHasher hasher, IDateTime clock)
    {
        _db = db;
        _hasher = hasher;
        _clock = clock;
    }

    public async Task<(RegisterResult Result, Customer? Customer)> RegisterAsync(
        string email, string password, string? fullName, CancellationToken ct = default)
    {
        var normalized = Normalize(email);
        if (await _db.Customers.AnyAsync(c => c.Email == normalized, ct))
            return (RegisterResult.EmailExists, null);

        var customer = new Customer
        {
            Email = normalized,
            PasswordHash = _hasher.Hash(password),
            FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim(),
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);
        return (RegisterResult.Ok, customer);
    }

    /// <summary>Trả Customer nếu email + mật khẩu đúng; ngược lại null.</summary>
    public async Task<Customer?> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default)
    {
        var normalized = Normalize(email);
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Email == normalized, ct);
        if (customer is null) return null;
        return _hasher.Verify(customer.PasswordHash, password) ? customer : null;
    }

    public Task<Customer?> GetByIdAsync(long id, CancellationToken ct = default)
        => _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);

    /// <summary>
    /// Sinh token đặt lại mật khẩu cho email (nếu tồn tại). Trả token THÔ (gửi qua email);
    /// chỉ lưu bản băm trong DB. Trả <c>null</c> nếu email không tồn tại (không tiết lộ).
    /// </summary>
    public async Task<string?> CreatePasswordResetTokenAsync(string email, CancellationToken ct = default)
    {
        var normalized = Normalize(email);
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Email == normalized, ct);
        if (customer is null) return null;

        var rawToken = GenerateToken();
        customer.ResetTokenHash = HashToken(rawToken);
        customer.ResetTokenExpiresAt = _clock.Now.Add(ResetTokenTtl);
        await _db.SaveChangesAsync(ct);
        return rawToken;
    }

    /// <summary>Đặt lại mật khẩu nếu (email, token) hợp lệ &amp; chưa hết hạn. Token dùng 1 lần.</summary>
    public async Task<bool> ResetPasswordAsync(string email, string rawToken, string newPassword, CancellationToken ct = default)
    {
        var normalized = Normalize(email);
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Email == normalized, ct);
        if (customer is null || customer.ResetTokenHash is null || customer.ResetTokenExpiresAt is null)
            return false;
        if (customer.ResetTokenExpiresAt < _clock.Now) return false;
        if (!FixedTimeEquals(customer.ResetTokenHash, HashToken(rawToken))) return false;

        customer.PasswordHash = _hasher.Hash(newPassword);
        customer.ResetTokenHash = null;
        customer.ResetTokenExpiresAt = null;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();

    private static string GenerateToken()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    private static string HashToken(string raw)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));

    private static bool FixedTimeEquals(string a, string b)
        => CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
}
