# P2-04 Tài khoản + lịch sử đơn hàng

- **Phase:** 2
- **Tính năng SPEC:** §3 (/customer/info, /customer/orders), §4.1 (quản lý tài khoản & lịch sử đơn), §6 UC-08
- **Trạng thái:** doing

## Mục tiêu
User đã đăng nhập xem dashboard tài khoản: **lịch sử đơn hàng** (mã, ngày, trạng thái, sản phẩm, tổng) +
tab **Thông tin** (họ tên + email, chỉ đọc).

## Phạm vi (in-scope)
- [ ] `/account` ([Authorize]) mở rộng thành dashboard: header (avatar chữ cái + tên + email + Đăng xuất)
      + tabs **Đơn hàng** / **Thông tin** (Alpine chuyển tab; cả hai server-render).
- [ ] Tab Đơn hàng: đơn của **chính user** (theo CustomerId), mới nhất trước. Mỗi đơn: mã `SD{Id:D6}`, ngày
      (CreatedAt), trạng thái (nhãn + thanh bước), danh sách item (tên SP + variant + SL + giá dòng từ
      **OrderItem.UnitPrice snapshot**), tổng. Empty state "chưa có đơn hàng".
- [ ] Tab Thông tin: Họ tên + Email (chỉ đọc).
- [ ] Helper thuần map `OrderStatus` → (nhãn tiếng Việt + chỉ số bước + đã huỷ?) cho thanh trạng thái (có test).

## Ngoài phạm vi (out-of-scope)
- **Yêu thích / Wishlist** → feature riêng (cần entity Wishlist + nút thả tim).
- Sửa hồ sơ / đổi mật khẩu (Info chỉ đọc). Hạng thành viên (chưa có membership).
- Trang chi tiết đơn riêng (/account/orders/{id}) — hiển thị inline trong list là đủ Phase 2.
- Huỷ đơn / mua lại / theo dõi vận chuyển realtime.
- Đơn của khách vãng lai (CustomerId null) — không có chỗ đăng nhập để xem (tra cứu đơn guest = feature sau).

## File sẽ đụng
- `src/HtMobile.Application/Features/Orders/OrderHistoryService.cs` + `Dtos/OrderHistoryDtos.cs` + `OrderStatusInfo.cs` (thuần).
- `src/HtMobile.Web/Areas/Storefront/Controllers/AccountController.cs` (Index nạp đơn + user info).
- `src/HtMobile.Web/Areas/Storefront/Views/Account/Index.cshtml` (viết lại thành dashboard) + `AccountVm`.
- `tests/HtMobile.Application.UnitTests/Orders/OrderStatusInfoTests.cs`.

## Phụ thuộc
- Order/OrderItem (đã có), Auth (P2-03: [Authorize], ICurrentUser), checkout tạo đơn (P2-02).

## Tiêu chí done
- [ ] Build xanh, Dependency Rule OK (DB qua IApplicationDbContext; lịch sử dùng giá snapshot, KHÔNG IPricingService).
- [ ] User đăng nhập → /account hiện đúng đơn của mình (mới nhất trước) + trạng thái + tổng; chưa có đơn → empty state.
- [ ] Tab Thông tin hiện họ tên + email. Tab chuyển mượt (Alpine), nội dung server-render.
- [ ] Anonymous → /account redirect /login (đã có từ Auth).
- [ ] Unit test OrderStatusInfo: map đủ các OrderStatus → nhãn + bước + cờ huỷ.

## Cách verify
- Đăng nhập → đặt 1 đơn (checkout) → /account tab Đơn hàng thấy đơn + trạng thái + tổng; tab Thông tin thấy email.
- `dotnet test` cho OrderStatusInfo.
