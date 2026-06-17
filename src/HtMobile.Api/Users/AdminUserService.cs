using HtMobile.Domain.Constants;
using HtMobile.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Api.Users;

public record UserRow(long Id, string Email, string? FullName, string[] Roles, bool LockedOut, bool EmailConfirmed);
public record CreateUserRequest(string Email, string? FullName, string Password, string[] Roles);
public record SetRolesRequest(string[] Roles);

public enum UserAdminResult { Ok, NotFound, SelfForbidden, LastAdmin, Invalid }

/// <summary>
/// Quản lý tài khoản + phân quyền (Identity) cho admin. Guard bảo mật: không tự đổi quyền/khoá chính mình,
/// không gỡ Admin / khoá Admin cuối cùng (tránh mất hết quản trị viên).
/// </summary>
public class AdminUserService
{
    private readonly UserManager<ApplicationUser> _users;

    public AdminUserService(UserManager<ApplicationUser> users) => _users = users;

    public async Task<List<UserRow>> ListAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var users = await _users.Users.OrderBy(u => u.Id).ToListAsync(ct);
        var rows = new List<UserRow>();
        foreach (var u in users)
        {
            var roles = await _users.GetRolesAsync(u);
            var locked = u.LockoutEnd is { } end && end > now;
            rows.Add(new UserRow(u.Id, u.Email ?? string.Empty, u.FullName, roles.ToArray(), locked, u.EmailConfirmed));
        }
        return rows;
    }

    public async Task<(UserAdminResult Result, string? Error, long Id)> CreateAsync(CreateUserRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return (UserAdminResult.Invalid, "Email và mật khẩu là bắt buộc.", 0);
        var roles = SanitizeRoles(req.Roles);
        if (await _users.FindByEmailAsync(req.Email) is not null)
            return (UserAdminResult.Invalid, "Email đã tồn tại.", 0);

        var user = new ApplicationUser { UserName = req.Email, Email = req.Email, FullName = req.FullName, EmailConfirmed = true };
        var created = await _users.CreateAsync(user, req.Password);
        if (!created.Succeeded)
            return (UserAdminResult.Invalid, string.Join(" ", created.Errors.Select(e => e.Description)), 0);
        if (roles.Length > 0) await _users.AddToRolesAsync(user, roles);
        return (UserAdminResult.Ok, null, user.Id);
    }

    public async Task<UserAdminResult> SetRolesAsync(long currentUserId, long id, string[] roles)
    {
        if (id == currentUserId) return UserAdminResult.SelfForbidden;
        var user = await _users.FindByIdAsync(id.ToString());
        if (user is null) return UserAdminResult.NotFound;

        var target = SanitizeRoles(roles);
        var current = await _users.GetRolesAsync(user);

        // Gỡ Admin của người khác → chặn nếu là Admin cuối.
        if (current.Contains(Roles.Admin) && !target.Contains(Roles.Admin) && await AdminCountAsync() <= 1)
            return UserAdminResult.LastAdmin;

        await _users.RemoveFromRolesAsync(user, current.Except(target));
        await _users.AddToRolesAsync(user, target.Except(current));
        return UserAdminResult.Ok;
    }

    public async Task<UserAdminResult> SetLockAsync(long currentUserId, long id, bool locked)
    {
        if (id == currentUserId) return UserAdminResult.SelfForbidden;
        var user = await _users.FindByIdAsync(id.ToString());
        if (user is null) return UserAdminResult.NotFound;

        if (locked && await _users.IsInRoleAsync(user, Roles.Admin) && await AdminCountAsync() <= 1)
            return UserAdminResult.LastAdmin;

        await _users.SetLockoutEnabledAsync(user, true);
        await _users.SetLockoutEndDateAsync(user, locked ? DateTimeOffset.UtcNow.AddYears(100) : null);
        return UserAdminResult.Ok;
    }

    private static string[] SanitizeRoles(string[]? roles)
        => (roles ?? Array.Empty<string>()).Where(r => Roles.All.Contains(r)).Distinct().ToArray();

    private async Task<int> AdminCountAsync()
        => (await _users.GetUsersInRoleAsync(Roles.Admin)).Count;
}
