using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HtMobile.Infrastructure.Identity;
using HtMobile.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HtMobile.Api.Auth;

/// <summary>Cấu hình JWT (bind từ section "Jwt"). Key để ở user-secrets/KeyVault khi deploy.</summary>
public class JwtOptions
{
    public string Issuer { get; set; } = "HtMobile";
    public string Audience { get; set; } = "HtMobile.Admin";
    public string Key { get; set; } = string.Empty;
    public int ExpireMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 14;
}

public record TokenPair(string AccessToken, DateTime AccessExpiresAt, string RefreshToken, DateTime RefreshExpiresAt);

/// <summary>
/// Phát hành/đối chiếu token cho Ht.Admin. Access = JWT ngắn hạn. Refresh = ĐA PHIÊN: mỗi phiên là 1 dòng
/// <see cref="RefreshToken"/> (lưu HASH); login tạo dòng mới (không đụng phiên khác), refresh xoay vòng
/// (revoke dòng cũ + tạo dòng mới), logout revoke đúng phiên. Token client = "{userId}.{random}".
/// </summary>
public class JwtTokenService
{
    private readonly JwtOptions _opt;
    private readonly UserManager<ApplicationUser> _users;
    private readonly AppDbContext _db;

    public JwtTokenService(IOptions<JwtOptions> opt, UserManager<ApplicationUser> users, AppDbContext db)
    {
        _opt = opt.Value;
        _users = users;
        _db = db;
    }

    /// <summary>Tạo access + 1 phiên refresh mới. Nếu <paramref name="rotateFrom"/> có giá trị → revoke phiên cũ đó.</summary>
    public async Task<TokenPair> IssueAsync(ApplicationUser user, string? rotateFrom = null, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        if (rotateFrom is not null && Parse(rotateFrom) is { } old && old.UserId == user.Id)
        {
            var oldRow = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == Sha256(old.Random) && t.RevokedAt == null, ct);
            if (oldRow is not null) oldRow.RevokedAt = now;
        }

        // Dọn phiên đã hết hạn của user (housekeeping rẻ).
        var expired = await _db.RefreshTokens.Where(t => t.UserId == user.Id && t.ExpiresAt < now).ToListAsync(ct);
        if (expired.Count > 0) _db.RefreshTokens.RemoveRange(expired);

        var (access, accessExp) = await CreateAccessTokenAsync(user);
        var random = Base64Url(RandomNumberGenerator.GetBytes(32));
        var refreshExp = now.AddDays(_opt.RefreshTokenDays);
        _db.RefreshTokens.Add(new RefreshToken { UserId = user.Id, TokenHash = Sha256(random), ExpiresAt = refreshExp, CreatedAt = now });
        await _db.SaveChangesAsync(ct);

        return new TokenPair(access, accessExp, $"{user.Id}.{random}", refreshExp);
    }

    /// <summary>Trả user nếu refresh token ứng với 1 phiên còn hiệu lực (chưa revoke, chưa hết hạn).</summary>
    public async Task<ApplicationUser?> ValidateRefreshAsync(string? refreshToken, CancellationToken ct = default)
    {
        if (Parse(refreshToken) is not { } p) return null;
        var hash = Sha256(p.Random);
        var now = DateTime.UtcNow;
        var row = await _db.RefreshTokens.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == hash && t.UserId == p.UserId && t.RevokedAt == null && t.ExpiresAt > now, ct);
        if (row is null) return null;
        return await _users.FindByIdAsync(p.UserId.ToString());
    }

    /// <summary>Revoke đúng 1 phiên (logout). Trả true nếu có phiên để revoke.</summary>
    public async Task<bool> RevokeAsync(string? refreshToken, CancellationToken ct = default)
    {
        if (Parse(refreshToken) is not { } p) return false;
        var hash = Sha256(p.Random);
        var row = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash && t.UserId == p.UserId && t.RevokedAt == null, ct);
        if (row is null) return false;
        row.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>Revoke toàn bộ phiên của 1 user (đổi mật khẩu / khoá tài khoản).</summary>
    public async Task RevokeAllAsync(long userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var rows = await _db.RefreshTokens.Where(t => t.UserId == userId && t.RevokedAt == null).ToListAsync(ct);
        foreach (var r in rows) r.RevokedAt = now;
        if (rows.Count > 0) await _db.SaveChangesAsync(ct);
    }

    private async Task<(string Token, DateTime ExpiresAt)> CreateAccessTokenAsync(ApplicationUser user)
    {
        var roles = await _users.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new("fullName", user.FullName ?? string.Empty),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_opt.ExpireMinutes);
        var token = new JwtSecurityToken(_opt.Issuer, _opt.Audience, claims, expires: expires, signingCredentials: creds);
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    private static (long UserId, string Random)? Parse(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        var dot = token.IndexOf('.');
        if (dot <= 0) return null;
        return long.TryParse(token[..dot], out var uid) ? (uid, token[(dot + 1)..]) : null;
    }

    private static string Sha256(string input) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
