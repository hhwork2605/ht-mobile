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
    public record RefreshRequest(string RefreshToken);
    public record AuthResponse(
        string AccessToken, DateTime AccessExpiresAt,
        string RefreshToken, DateTime RefreshExpiresAt,
        string Email, string FullName, string[] Roles);

    /// <summary>Đăng nhập admin → cấp access + refresh token. Chỉ tài khoản role Admin.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _users.FindByEmailAsync(req.Email);
        if (user is null || !await _users.CheckPasswordAsync(user, req.Password))
            return Unauthorized(new { message = "Email hoặc mật khẩu không đúng." });
        if (await _users.IsLockedOutAsync(user))
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Tài khoản đang bị khoá." });

        var roles = await _users.GetRolesAsync(user);
        if (!roles.Contains(Roles.Admin))
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Tài khoản không có quyền quản trị." });

        return Ok(await BuildResponseAsync(user, roles));
    }

    /// <summary>Cấp lại access token bằng refresh token (xoay vòng đúng phiên đó).</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        var user = await _jwt.ValidateRefreshAsync(req.RefreshToken);
        if (user is null) return Unauthorized(new { message = "Phiên đăng nhập đã hết hạn." });

        var roles = await _users.GetRolesAsync(user);
        if (!roles.Contains(Roles.Admin) || await _users.IsLockedOutAsync(user))
        {
            await _jwt.RevokeAllAsync(user.Id);   // mất quyền/bị khoá → revoke mọi phiên
            return Unauthorized(new { message = "Tài khoản không còn quyền truy cập." });
        }

        return Ok(await BuildResponseAsync(user, roles, rotateFrom: req.RefreshToken));
    }

    /// <summary>Đăng xuất: thu hồi đúng phiên (refresh token) hiện tại. Phiên khác không ảnh hưởng.</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
    {
        await _jwt.RevokeAsync(req.RefreshToken);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
    public async Task<IActionResult> Me()
    {
        var user = await _users.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var roles = await _users.GetRolesAsync(user);
        return Ok(new { email = user.Email, fullName = user.FullName, roles });
    }

    private async Task<AuthResponse> BuildResponseAsync(ApplicationUser user, IList<string> roles, string? rotateFrom = null)
    {
        var pair = await _jwt.IssueAsync(user, rotateFrom);
        return new AuthResponse(
            pair.AccessToken, pair.AccessExpiresAt, pair.RefreshToken, pair.RefreshExpiresAt,
            user.Email ?? string.Empty, user.FullName ?? string.Empty, roles.ToArray());
    }
}
