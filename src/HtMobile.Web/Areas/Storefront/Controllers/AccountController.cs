using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Customers;
using HtMobile.Application.Features.Orders;
using HtMobile.Web.Infrastructure;
using HtMobile.Web.Models.Account;
using HtMobile.Web.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HtMobile.Web.Areas.Storefront.Controllers;

/// <summary>
/// Auth storefront bằng tài khoản <c>Customer</c> (cookie riêng, không qua ASP.NET Identity — ADR 0004):
/// đăng ký / đăng nhập / đăng xuất / quên-đặt lại mật khẩu + landing tài khoản.
/// </summary>
[Area("Storefront")]
public class AccountController : Controller
{
    private readonly CustomerAccountService _accounts;
    private readonly ICurrentUser _currentUser;
    private readonly ICartService _cart;
    private readonly CartContext _cartCtx;
    private readonly IEmailSender _email;
    private readonly OrderHistoryService _orders;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        CustomerAccountService accounts,
        ICurrentUser currentUser,
        ICartService cart,
        CartContext cartCtx,
        IEmailSender email,
        OrderHistoryService orders,
        ILogger<AccountController> logger)
    {
        _accounts = accounts;
        _currentUser = currentUser;
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
    public async Task<IActionResult> Login(LoginVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var customer = await _accounts.ValidateCredentialsAsync(vm.Email, vm.Password, ct);
        if (customer is not null)
        {
            await StorefrontAuth.SignInAsync(HttpContext, customer, vm.RememberMe);
            await MergeGuestCartAsync(customer.Id);
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
    public async Task<IActionResult> Register(RegisterVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var (result, customer) = await _accounts.RegisterAsync(vm.Email, vm.Password, vm.FullName, ct);
        if (result == RegisterResult.Ok && customer is not null)
        {
            await StorefrontAuth.SignInAsync(HttpContext, customer, isPersistent: false);
            await MergeGuestCartAsync(customer.Id);
            return Redirect(UrlSafety.SafeLocalUrl(vm.ReturnUrl));
        }

        ModelState.AddModelError(nameof(RegisterVm.Email), "Email này đã được đăng ký.");
        return View(vm);
    }

    [HttpPost("/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await StorefrontAuth.SignOutAsync(HttpContext);
        return Redirect("/");
    }

    [HttpGet("/forgot-password")]
    public IActionResult ForgotPassword() => View(new ForgotPasswordVm());

    [HttpPost("/forgot-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        var token = await _accounts.CreatePasswordResetTokenAsync(vm.Email, ct);
        if (token is not null)
        {
            var link = $"{Request.Scheme}://{Request.Host}/reset-password" +
                       $"?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(vm.Email)}";
            await _email.SendAsync(vm.Email, "Đặt lại mật khẩu HtMobile",
                $"<p>Nhấn vào liên kết để đặt lại mật khẩu:</p><p><a href=\"{link}\">{link}</a></p>", ct);
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
    public async Task<IActionResult> ResetPassword(ResetPasswordVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);

        if (await _accounts.ResetPasswordAsync(vm.Email, vm.Token, vm.Password, ct))
        {
            ViewBag.Done = true;
            return View(vm);
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
        if (_currentUser.UserId is not long customerId) return Redirect("/login");
        var customer = await _accounts.GetByIdAsync(customerId, ct);
        if (customer is null) return Redirect("/login");

        var orders = await _orders.GetMyOrdersAsync(customerId, ct);
        return View(new AccountVm
        {
            FullName = customer.FullName,
            Email = customer.Email,
            Orders = orders
        });
    }

    private async Task MergeGuestCartAsync(long customerId)
    {
        var guestId = _cartCtx.GuestSessionId;
        if (string.IsNullOrEmpty(guestId)) return;

        // Best-effort: merge giỏ KHÔNG được làm hỏng đăng nhập đã thành công.
        try
        {
            await _cart.MergeAsync(guestId, customerId);
            _cartCtx.ClearGuest();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Merge giỏ guest→customer {CustomerId} thất bại — bỏ qua.", customerId);
        }
    }
}
