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

/// <summary>Vùng giá đang chọn (cookie/session). Mặc định: miền Bắc.</summary>
public interface ICurrentRegion
{
    long RegionId { get; }
}

/// <summary>Đồng hồ hệ thống — tách ra để test được logic phụ thuộc thời gian. Trả giờ server (DateTime.Now).</summary>
public interface IDateTime
{
    DateTime Now { get; }
}
