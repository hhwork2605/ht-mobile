using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HtMobile.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HtMobile.Api.Auth;

/// <summary>Cấu hình JWT (bind từ section "Jwt"). Key để ở user-secrets/appsettings khi triển khai thật.</summary>
public class JwtOptions
{
    public string Issuer { get; set; } = "HtMobile";
    public string Audience { get; set; } = "HtMobile.Admin";
    public string Key { get; set; } = string.Empty;
    public int ExpireMinutes { get; set; } = 480;
}

/// <summary>Phát hành JWT cho admin (Ht.Admin Angular gọi API). Claim: sub, email, name, role.</summary>
public class JwtTokenService
{
    private readonly JwtOptions _opt;
    private readonly UserManager<ApplicationUser> _users;

    public JwtTokenService(IOptions<JwtOptions> opt, UserManager<ApplicationUser> users)
    {
        _opt = opt.Value;
        _users = users;
    }

    public async Task<(string Token, DateTime ExpiresAt)> CreateAsync(ApplicationUser user)
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

        var token = new JwtSecurityToken(
            issuer: _opt.Issuer, audience: _opt.Audience,
            claims: claims, expires: expires, signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
