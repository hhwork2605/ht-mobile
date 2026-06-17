using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Orders;
using HtMobile.Domain.Constants;
using HtMobile.Infrastructure.Identity;
using HtMobile.Web.Infrastructure;
using HtMobile.Web.Models.Account;
using HtMobile.Web.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Storefront.Controllers;

/// <summary>Auth (Identity cookie): đăng ký / đăng nhập / đăng xuất / quên-đặt lại mật khẩu + landing tài khoản.</summary>
[Area("Storefront")]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly ICartService _cart;
    private readonly CartContext _cartCtx;
    private readonly IEmailSender _email;
    private readonly OrderHistoryService _orders;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn,
        ICartService cart,
        CartContext cartCtx,
        IEmailSender email,
        OrderHistoryService orders,
        ILogger<AccountController> logger)
    {
        _users = users;
        _signIn = signIn;
        _cart = cart;
        _cartCtx = cartCtx;
        _email = email;
        _orders = orders;
        _logger = logger;
    }

    [HttpGet("/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return Redirect(UrlSafety.SafeLocalUrl(returnUrl));
        return View(new LoginVm { ReturnUrl = returnUrl });
    }

    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // Dùng overload theo userName: xử lý đồng nhất dù email tồn tại hay không (tránh lộ qua timing).
        var result = await _signIn.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            var user = await _users.FindByEmailAsync(vm.Email);
            if (user is not null) await MergeGuestCartAsync(user.Id);
            return Redirect(UrlSafety.SafeLocalUrl(vm.ReturnUrl));
        }

        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
        return View(vm);
    }

    [HttpGet("/register")]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return Redirect(UrlSafety.SafeLocalUrl(returnUrl));
        return View(new RegisterVm { ReturnUrl = returnUrl });
    }

    [HttpPost("/register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = new ApplicationUser { UserName = vm.Email, Email = vm.Email, FullName = vm.FullName };
        var result = await _users.CreateAsync(user, vm.Password);
        if (result.Succeeded)
        {
            await _users.AddToRoleAsync(user, Roles.Customer);
            await _signIn.SignInAsync(user, isPersistent: false);
            await MergeGuestCartAsync(user.Id);
            return Redirect(UrlSafety.SafeLocalUrl(vm.ReturnUrl));
        }

        foreach (var e in result.Errors)
            ModelState.AddModelError(string.Empty, e.Description);
        return View(vm);
    }

    [HttpPost("/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return Redirect("/");
    }

    [HttpGet("/forgot-password")]
    public IActionResult ForgotPassword() => View(new ForgotPasswordVm());

    [HttpPost("/forgot-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _users.FindByEmailAsync(vm.Email);
        if (user is not null)
        {
            var token = await _users.GeneratePasswordResetTokenAsync(user);
            var link = $"{Request.Scheme}://{Request.Host}/reset-password" +
                       $"?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(vm.Email)}";
            await _email.SendAsync(vm.Email, "Đặt lại mật khẩu HtMobile",
                $"<p>Nhấn vào liên kết để đặt lại mật khẩu:</p><p><a href=\"{link}\">{link}</a></p>");
            // Dev (NullEmailSender không gửi thật) — log link để test được.
            _logger.LogInformation("[Reset password] email={Email} link={Link}", vm.Email, link);
        }

        // KHÔNG tiết lộ email có tồn tại hay không.
        ViewBag.Sent = true;
        return View(vm);
    }

    [HttpGet("/reset-password")]
    public IActionResult ResetPassword(string? token, string? email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            return BadRequest();
        return View(new ResetPasswordVm { Token = token, Email = email });
    }

    [HttpPost("/reset-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _users.FindByEmailAsync(vm.Email);
        if (user is not null)
        {
            var result = await _users.ResetPasswordAsync(user, vm.Token, vm.Password);
            if (result.Succeeded)
            {
                ViewBag.Done = true;
                return View(vm);
            }
        }

        // Mọi trường hợp khác (email lạ HOẶC token sai/hết hạn) → thông báo CHUNG, không lộ email tồn tại.
        ModelState.AddModelError(string.Empty,
            "Liên kết không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu đặt lại mật khẩu mới.");
        return View(vm);
    }

    [Authorize]
    [HttpGet("/account")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var user = await _users.GetUserAsync(User);
        if (user is null) return Redirect("/login");

        var orders = await _orders.GetMyOrdersAsync(user.Id, ct);
        return View(new AccountVm
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Orders = orders
        });
    }

    private async Task MergeGuestCartAsync(long userId)
    {
        var guestId = _cartCtx.GuestSessionId;
        if (string.IsNullOrEmpty(guestId)) return;

        // Best-effort: merge giỏ KHÔNG được làm hỏng đăng nhập đã thành công.
        try
        {
            await _cart.MergeAsync(guestId, userId);
            _cartCtx.ClearGuest();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Merge giỏ guest→user {UserId} thất bại — bỏ qua.", userId);
        }
    }
}
