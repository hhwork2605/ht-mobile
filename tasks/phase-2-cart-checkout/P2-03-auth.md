# P2-03 Auth (Đăng ký / Đăng nhập / Quên mật khẩu / Nhớ đăng nhập)

- **Phase:** 2
- **Tính năng SPEC:** §3 (/login, /register), §4.1 (đăng nhập/đăng ký/quên MK/nhớ ĐN), §13 (bảo mật)
- **Trạng thái:** doing

## Mục tiêu
Khách tạo tài khoản và đăng nhập bằng **email + mật khẩu** (ASP.NET Identity, cookie), nhớ đăng nhập,
đăng xuất, và đặt lại mật khẩu khi quên. Khi đăng nhập/đăng ký thành công thì **gộp giỏ guest vào giỏ user**.

## Phạm vi (in-scope)
- [ ] Đăng ký `/register`: email + mật khẩu + họ tên → tạo `ApplicationUser`, gán role `Customer`, đăng nhập luôn.
- [ ] Đăng nhập `/login?ReturnUrl=`: email + mật khẩu + "Nhớ đăng nhập" (cookie bền). Lỗi sai → thông báo chung.
- [ ] Đăng xuất `/logout` (POST, antiforgery).
- [ ] Quên mật khẩu `/forgot-password` → sinh token reset + gọi `IEmailSender`; `/reset-password` (token + MK mới).
- [ ] Chống open-redirect: `ReturnUrl` chỉ nhận URL nội bộ (helper thuần, có test).
- [ ] **Gộp giỏ**: sau đăng nhập/đăng ký gọi `ICartService.MergeAsync(guestSessionId, userId)` rồi xoá cookie guest.
- [ ] UI theo card ShopDunk (icon tròn, h1, input, nút accent) + design tokens; antiforgery mọi form.
- [ ] Header: icon tài khoản → `/account` khi đã đăng nhập (trang tối thiểu: email + nút Đăng xuất).

## Ngoài phạm vi (out-of-scope)
- Dashboard tài khoản đầy đủ (đơn hàng/yêu thích/thông tin tabs) → feature "Tài khoản + lịch sử đơn" riêng.
  `/account` ở đây chỉ là landing tối thiểu để có chỗ Đăng xuất.
- Gửi email thật: `IEmailSender` hiện là `NullEmailSender` → token reset KHÔNG tới hộp thư. Dev: log link reset.
- Xác thực email, đăng nhập mạng xã hội, 2FA, OTP/SMS.
- Rate-limit login (SPEC §13) — ghi nhận, làm ở Phase hoàn thiện.

## File sẽ đụng
- `src/HtMobile.Web/Areas/Storefront/Controllers/AccountController.cs` (mới): login/register/logout/forgot/reset/account.
- `src/HtMobile.Web/Areas/Storefront/Views/Account/*.cshtml` (Login, Register, ForgotPassword, ResetPassword, Index).
- `src/HtMobile.Web/Models/Auth/*Vm.cs` (ViewModel + DataAnnotations validate).
- `src/HtMobile.Web/Infrastructure/UrlSafety.cs` (mới) — `SafeLocalUrl(url)` chống open-redirect (logic thuần, test).
- `src/HtMobile.Web/Infrastructure/CartContext.cs` — thêm đọc guest session id + xoá cookie guest (cho merge).
- `src/HtMobile.Web/Views/Shared/_Layout.cshtml` — link tài khoản → `/account` khi đã đăng nhập.
- `tests/HtMobile.Application.UnitTests/...` hoặc Web test — test `SafeLocalUrl`.

## Phụ thuộc
- ASP.NET Identity đã cấu hình (Infrastructure DI: AddIdentity, cookie, LoginPath=/login). `ApplicationUser.FullName` đã có.
- Role `Customer` (Roles.All). `ICartService.MergeAsync` (đã có từ P2-01).

## Tiêu chí done
- [ ] Build xanh, không vi phạm Dependency Rule (Web dùng UserManager/SignInManager — framework service, hợp lệ).
- [ ] Đăng ký → tự đăng nhập; Đăng nhập đúng/sai hoạt động; "Nhớ đăng nhập" ra cookie bền; Đăng xuất OK.
- [ ] Quên MK sinh token + (dev) log link reset; Reset MK đổi được mật khẩu.
- [ ] Giỏ guest được gộp vào giỏ user sau đăng nhập (kiểm bằng thêm SP khi chưa đăng nhập → đăng nhập → giỏ còn).
- [ ] `ReturnUrl` ngoại lai bị bỏ (về "/"); test `SafeLocalUrl` xanh.
- [ ] Forgot-password KHÔNG lộ email có tồn tại hay không (luôn báo "đã gửi").

## Cách verify
- Chạy app: /register tạo tài khoản → /login → header thành "đã đăng nhập" → /account → Đăng xuất.
- Thêm SP lúc chưa đăng nhập → đăng nhập → mở /cart kiểm giỏ vẫn còn (merge).
- `dotnet test` cho `SafeLocalUrl`.
