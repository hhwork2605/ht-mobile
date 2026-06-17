using HtMobile.Api.Users;
using HtMobile.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HtMobile.Infrastructure.Identity;

namespace HtMobile.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = Roles.Admin)]
public class UsersController : ControllerBase
{
    private readonly AdminUserService _service;
    private readonly UserManager<ApplicationUser> _users;

    public UsersController(AdminUserService service, UserManager<ApplicationUser> users)
    {
        _service = service;
        _users = users;
    }

    private long CurrentUserId => long.TryParse(_users.GetUserId(User), out var id) ? id : 0;

    [HttpGet]
    public async Task<ActionResult<List<UserRow>>> List(CancellationToken ct) => Ok(await _service.ListAsync(ct));

    [HttpGet("roles")]
    public ActionResult<string[]> RolesList() => Ok(Roles.All);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest req)
    {
        var (result, error, id) = await _service.CreateAsync(req);
        return result == UserAdminResult.Ok ? Created($"/api/users/{id}", new { id })
            : BadRequest(new { message = error ?? "Tạo tài khoản thất bại." });
    }

    [HttpPut("{id:long}/roles")]
    public async Task<IActionResult> SetRoles(long id, [FromBody] SetRolesRequest req)
        => Map(await _service.SetRolesAsync(CurrentUserId, id, req.Roles));

    [HttpPost("{id:long}/lock")]
    public async Task<IActionResult> Lock(long id) => Map(await _service.SetLockAsync(CurrentUserId, id, true));

    [HttpPost("{id:long}/unlock")]
    public async Task<IActionResult> Unlock(long id) => Map(await _service.SetLockAsync(CurrentUserId, id, false));

    private IActionResult Map(UserAdminResult r) => r switch
    {
        UserAdminResult.Ok => NoContent(),
        UserAdminResult.NotFound => NotFound(),
        UserAdminResult.SelfForbidden => Conflict(new { message = "Không thể thao tác trên chính tài khoản đang đăng nhập." }),
        UserAdminResult.LastAdmin => Conflict(new { message = "Không thể gỡ quyền/khoá quản trị viên cuối cùng." }),
        _ => BadRequest(new { message = "Yêu cầu không hợp lệ." }),
    };
}
