# Tiến độ: Auth (Đăng ký / Đăng nhập / Quên MK / Nhớ đăng nhập) — Phase 2
slug: auth
Cập nhật: 2026-06-16 12:10
Cổng hiện tại: 5

## Trạng thái cổng
- [x] 0. Thiết kế/UI      — DONE (email+MK Identity; tái dùng card ShopDunk)
- [x] 1. Spec            — DONE (duyệt; quên MK log link ở dev)
- [x] 2. Contract        — DONE (duyệt; KHÔNG đổi schema)
- [x] 3. Test (red)      — DONE (12 test UrlSafety đỏ)
- [x] 4. Implement       — DONE (build xanh, 36 test pass)
- [x] 5. Review subagent — DONE (0 Critical; xem Vấn đề mở)
- [x] 6. Review tay       — DONE (duyệt fix H1/H2/M2/L1; M1 dương tính giả)
- [x] 7. Test (green)     — DONE (36 test + smoke-test runtime)
- [x] 8. Commit           — IN_PROGRESS

## Vấn đề mở (reviewer Cổng 5)
- [H1] Login lộ user-enumeration qua timing (bỏ băm khi email không tồn tại) → dùng overload PasswordSignInAsync(email,...).
- [H2] MergeGuestCartAsync chưa try/catch → lỗi merge làm hỏng đăng nhập đã thành công → bọc best-effort.
- [M2] Reset-password phân biệt user tồn tại qua lỗi token → thông báo chung cho mọi token sai.
- [M1] _Layout href "\cart" (backslash) → đổi "/cart".
- [L1] Comment "hết hạn ít phút" sai (token Identity ~24h) → sửa câu chữ.
- [L2] NullEmailSender log token mọi env (chỉ là dev-sender) — ghi nhận, thay ở phase Dịch vụ.
- Test: chưa có functional test register→login→merge (merge đã test ở CartService; smoke-test sẽ phủ).
- Điểm tốt: open-redirect chặn đúng + áp mọi redirect; antiforgery đủ; forgot không lộ email; userId qua FindByEmailAsync.
- [ ] 6. Review tay       — TODO
- [ ] 7. Test (green)    — TODO
- [ ] 8. Commit          — TODO

## Artifact đã tạo
- docs/features/auth.progress.md

## Thiết kế (Cổng 0) — từ bundle ShopDunk (đã có sẵn)
- Màn account (dòng 507-590, ShopDunk Storefront.dc.html):
  - Logged out: card giữa (max 380px), icon tròn user, h1 "Đăng nhập", phụ đề, input + nút "Đăng nhập".
    Prototype DEMO dùng tên+SĐT; stack thật = Identity email+mật khẩu → cần điều chỉnh.
  - Logged in: avatar (chữ cái) + tên + SĐT + "Đăng xuất"; tab Đơn hàng/Yêu thích/Thông tin.
    → Dashboard tài khoản này thuộc backlog item "Tài khoản + lịch sử đơn" (KHÁC feature Auth).

## Quyết định chính
- Phạm vi Auth = Đăng ký / Đăng nhập (+ Nhớ đăng nhập) / Đăng xuất / Quên mật khẩu (+ reset). KHÔNG gồm
  dashboard tài khoản (orders/wishlist/info) — đó là feature riêng.
- Nối CartService.MergeAsync(sessionId, userId) sau khi đăng nhập/đăng ký thành công (hook đã dựng ở P2-01).

## Vấn đề mở (chờ chốt Cổng 0/1)
- Phương thức đăng nhập: email+mật khẩu (Identity mặc định) hay theo prototype (SĐT)?
- "Quên mật khẩu": gửi email reset — nhưng IEmailSender hiện là NullEmailSender (chưa gửi thật).

## Contract (Cổng 2) — chốt
- KHÔNG đổi schema (Identity tables + ApplicationUser.FullName đã có). → KHÔNG migration.
- Web: AccountController (Storefront) — routes /register /login /logout /forgot-password /reset-password /account.
- ViewModel: Web/Models/Auth/{Login,Register,ForgotPassword,ResetPassword}Vm + DataAnnotations.
- Helper thuần: UrlSafety.SafeLocalUrl(url) chống open-redirect (Cổng 3 test).
- CartContext: thêm GuestSessionId (đọc) + ClearGuest() để merge giỏ sau đăng nhập.
- Dùng UserManager/SignInManager (framework, Web hợp lệ). Lấy userId qua FindByEmailAsync (không chờ CurrentUser).
- NullEmailSender: log thêm htmlBody (để thấy link reset ở dev).
- Header _Layout: icon tài khoản → /account khi đã đăng nhập.
- Docs: api.md đã thêm mục Tài khoản/Auth.

## Đã sửa (Cổng 7) + smoke-test
- H1 login overload (hết timing-leak), H2 merge best-effort try/catch, M2 reset thông báo chung, L1 câu chữ.
  M1 (\cart) là dương tính giả — đã là /cart.
- Smoke-test runtime OK: guest add→register→**merge giỏ giữ SP**→htm_cart cleared; /account hiện email;
  /account ẩn danh→302 /login?ReturnUrl=/account; ReturnUrl=evil bị chặn về /; sai MK ở lại form;
  forgot-password log link reset.

## Next action
- XONG feature Auth. Phase 2 còn: Checkout (P2-02), Tài khoản đầy đủ, Pricing engine đầy đủ.
