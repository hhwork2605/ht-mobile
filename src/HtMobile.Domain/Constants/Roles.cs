namespace HtMobile.Domain.Constants;

public static class Roles
{
    public const string Admin = "Admin";

    // Role Identity chỉ còn dùng cho đăng nhập Admin (ADR 0005). Khách hàng storefront ở bảng Customers,
    // KHÔNG dùng role Identity nữa → role "Customer" cũ đã bỏ.
    public static readonly string[] All = { Admin };
}
