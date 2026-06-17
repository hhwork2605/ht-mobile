using HtMobile.Api.Auth;
using HtMobile.Domain.Constants;
using HtMobile.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly JwtTokenService _jwt;

    public AuthController(UserManager<ApplicationUser> users, JwtTokenService jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    public record LoginRequest(string Email, string Password);
    public record LoginResponse(string Token, DateTime ExpiresAt, string Email, string FullName, string[] Roles);

    /// <summary>Đăng nhập admin → cấp JWT. Chỉ tài khoản role Admin mới được cấp token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _users.FindByEmailAsync(req.Email);
        if (user is null || !await _users.CheckPasswordAsync(user, req.Password))
            return Unauthorized(new { message = "Email hoặc mật khẩu không đúng." });

        var roles = await _users.GetRolesAsync(user);
        if (!roles.Contains(Roles.Admin))
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Tài khoản không có quyền quản trị." });

        var (token, expires) = await _jwt.CreateAsync(user);
        return Ok(new LoginResponse(token, expires, user.Email ?? string.Empty, user.FullName ?? string.Empty, roles.ToArray()));
    }

    /// <summary>Thông tin tài khoản hiện tại (kiểm tra token còn hợp lệ).</summary>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
    public async Task<IActionResult> Me()
    {
        var user = await _users.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var roles = await _users.GetRolesAsync(user);
        return Ok(new { email = user.Email, fullName = user.FullName, roles });
    }
}
