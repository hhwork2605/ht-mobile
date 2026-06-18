namespace HtMobile.Application.Common.Interfaces;

/// <summary>Gửi email (xác nhận đơn, quên mật khẩu, theo dõi hàng về…). Stub ở Infrastructure.</summary>
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}

/// <summary>Thông tin người dùng hiện tại (lấy từ HttpContext ở Web).</summary>
public interface ICurrentUser
{
    long? UserId { get; }
    bool IsAuthenticated { get; }
}

/// <summary>Đồng hồ hệ thống — tách ra để test được logic phụ thuộc thời gian. Trả giờ server (DateTime.Now).</summary>
public interface IDateTime
{
    DateTime Now { get; }
}

/// <summary>Băm/kiểm tra mật khẩu (tách khỏi ASP.NET — hiện thực ở Infrastructure). Dùng cho tài khoản Customer storefront.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    /// <summary><c>true</c> nếu <paramref name="password"/> khớp <paramref name="hash"/>.</summary>
    bool Verify(string hash, string password);
}
