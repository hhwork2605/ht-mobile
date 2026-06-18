# ADR 0005 — Tách auth storefront (Customer) khỏi Identity (admin)

- Trạng thái: Accepted
- Ngày: 2026-06-18
- Liên quan: [0001](0001-clean-architecture.md), [0003](0003-product-attribute-eav.md), [0004](0004-admin-api-angular.md)

## Bối cảnh

Trước đây cả khách hàng storefront lẫn quản trị viên đều là `ApplicationUser : IdentityUser<long>`
(một bảng `AspNetUsers`), phân biệt bằng role. Sau khi admin tách thành app Angular + API JWT riêng
(ADR 0004), việc gộp chung hai loại tài khoản gây ràng buộc không cần thiết: storefront phải mang theo
toàn bộ hạ tầng Identity (cookie Identity, lockout, token providers…) dù nhu cầu chỉ là đăng nhập đơn giản;
mô hình dữ liệu khách hàng bị khoá vào schema Identity.

## Quyết định

Tách **hai loại tài khoản**:

- **`Customer`** (bảng `Customers`, `Domain/Entities/Customers`) — tài khoản **storefront**. Đăng nhập email +
  mật khẩu, băm qua `IPasswordHasher` (hiện thực `IdentityPasswordHasher` = PBKDF2 của ASP.NET, không kéo theo
  Identity store). Web đăng nhập bằng **cookie scheme riêng `"Storefront"`** (`StorefrontAuth`), claim
  `NameIdentifier = Customer.Id` nên `ICurrentUser`/`CartContext`/`CheckoutService`/`OrderHistoryService` dùng lại
  nguyên vẹn. Nghiệp vụ ở `CustomerAccountService` (Application): đăng ký / kiểm tra đăng nhập / reset mật khẩu
  (token băm SHA-256 + hạn 1 giờ).
- **`ApplicationUser`** (Identity, `AspNetUsers`) — **chỉ còn cho đăng nhập Admin** qua `HtMobile.Api` (JWT Bearer).

`Order.CustomerId` đổi thành **FK cứng → `Customers.Id`** (`OnDelete: Restrict`, `null` = guest). Các cột
`CustomerId` khác (Cart/Review/Address/TradeIn) mang ngữ nghĩa `Customers.Id` (chưa đặt FK cứng — phạm vi sau).

Trong app Web (storefront-only) phải **ghi đè tường minh** `DefaultAuthenticate/Challenge/SignIn scheme` sang
`"Storefront"` vì `AddInfrastructure` (dùng chung với API) gọi `AddIdentity` đặt sẵn các scheme đó về cookie Identity.

## Hệ quả

- Storefront không phụ thuộc Identity; mô hình `Customer` tự do mở rộng (hạng thành viên, điểm thưởng…).
- Migration `AddCustomers`: tạo bảng `Customers` + FK `Orders → Customers`. Đơn/giỏ cũ có `CustomerId` trỏ tới
  `AspNetUsers` (trước ADR) được **chuyển về `null` (guest)** trong migration để FK mới hợp lệ (không drop được DB
  managed Supabase).
- Identity vẫn cần thiết cho admin → giữ `AddIdentity` trong Infrastructure dùng chung.
- Reset mật khẩu storefront tự quản token (không qua Identity token providers).
