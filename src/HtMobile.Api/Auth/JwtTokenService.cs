using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HtMobile.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HtMobile.Api.Auth;

/// <summary>Cấu hình JWT (bind từ section "Jwt"). Key để ở user-secrets/KeyVault khi deploy.</summary>
public class JwtOptions
{
    public string Issuer { get; set; } = "HtMobile";
    public string Audience { get; set; } = "HtMobile.Admin";
    public string Key { get; set; } = string.Empty;
    public int ExpireMinutes { get; set; } = 15;        // access token ngắn hạn
    public int RefreshTokenDays { get; set; } = 14;      // refresh token dài hạn
}

/// <summary>Cặp token trả về client.</summary>
public record TokenPair(string AccessToken, DateTime AccessExpiresAt, string RefreshToken, DateTime RefreshExpiresAt);

/// <summary>
/// Phát hành/đối chiếu token cho Ht.Admin. Access = JWT ngắn hạn. Refresh = chuỗi ngẫu nhiên dài hạn,
/// LƯU DẠNG HASH ở AspNetUserTokens (Identity) — không cần bảng/migration mới; xoay vòng mỗi lần refresh,
/// thu hồi khi logout. Refresh token gửi cho client có dạng "{userId}.{random}" để tra user mà không quét.
/// </summary>
public class JwtTokenService
{
    private const string Provider = "HtMobile";
    private const string RefreshName = "refresh";

    private readonly JwtOptions _opt;
    private readonly UserManager<ApplicationUser> _users;

    public JwtTokenService(IOptions<JwtOptions> opt, UserManager<ApplicationUser> users)
    {
        _opt = opt.Value;
        _users = users;
    }

    /// <summary>Tạo access + refresh mới, lưu hash refresh (ghi đè refresh cũ = xoay vòng).</summary>
    public async Task<TokenPair> IssueAsync(ApplicationUser user)
    {
        var (access, accessExp) = await CreateAccessTokenAsync(user);

        var random = Base64Url(RandomNumberGenerator.GetBytes(32));
        var refreshExp = DateTime.UtcNow.AddDays(_opt.RefreshTokenDays);
        var stored = $"{Sha256(random)}:{new DateTimeOffset(refreshExp).ToUnixTimeSeconds()}";
        await _users.RemoveAuthenticationTokenAsync(user, Provider, RefreshName);
        await _users.SetAuthenticationTokenAsync(user, Provider, RefreshName, stored);

        return new TokenPair(access, accessExp, $"{user.Id}.{random}", refreshExp);
    }

    /// <summary>Đối chiếu refresh token; trả user nếu hợp lệ (chưa hết hạn, khớp hash), ngược lại null.</summary>
    public async Task<ApplicationUser?> ValidateRefreshAsync(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;
        var dot = refreshToken.IndexOf('.');
        if (dot <= 0) return null;
        if (!long.TryParse(refreshToken[..dot], out var userId)) return null;
        var random = refreshToken[(dot + 1)..];

        var user = await _users.FindByIdAsync(userId.ToString());
        if (user is null) return null;

        var stored = await _users.GetAuthenticationTokenAsync(user, Provider, RefreshName);
        if (string.IsNullOrEmpty(stored)) return null;
        var sep = stored.LastIndexOf(':');
        if (sep <= 0) return null;

        var hash = stored[..sep];
        if (!long.TryParse(stored[(sep + 1)..], out var expUnix)) return null;
        if (DateTimeOffset.FromUnixTimeSeconds(expUnix) < DateTimeOffset.UtcNow) return null;

        var presented = Sha256(random);
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(hash), Encoding.UTF8.GetBytes(presented)))
            return null;

        return user;
    }

    public Task RevokeAsync(ApplicationUser user) => _users.RemoveAuthenticationTokenAsync(user, Provider, RefreshName);

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

    private static string Sha256(string input)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));

    private static string Base64Url(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
