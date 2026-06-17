# Tiến độ: Tài khoản + lịch sử đơn hàng — Phase 2
slug: account
Cập nhật: 2026-06-17 00:40
Cổng hiện tại: 2

## Trạng thái cổng
- [x] 0. Thiết kế/UI      — DONE (Đơn hàng + Thông tin; bỏ Wishlist; Info chỉ đọc)
- [x] 1. Spec            — DONE (duyệt)
- [x] 2. Contract        — DONE (duyệt; KHÔNG đổi schema)
- [x] 3. Test (red)      — DONE (8 test OrderStatusInfo đỏ)
- [x] 4. Implement       — DONE (build xanh, 49 test pass)
- [x] 5. Review subagent — DONE (0 blocker; quyền/giá đúng)
- [x] 6. Review tay       — DONE (duyệt fix H1/M2/L2)
- [x] 7. Test (green)     — DONE (49 test pass)
- [x] 8. Commit           — DONE (6524a0f)

## Đã sửa (Cổng 7)
- H1: tên user thành <h1>. M2: thumbnail ưu tiên ảnh đúng variant. L2: link tên SP trong đơn về PDP.
- (Build từng kẹt do tiến trình HtMobile.Web.exe sót khoá DLL → đã kill, build lại xanh.)

## Hoàn tất
- Migration OrderCustomerNullable ĐÃ apply. Smoke-test E2E OK: đăng ký → giỏ → /checkout (302 success
  SD000002) → /account hiện đơn #SD000002 + SP + trạng thái "Chờ xác nhận"; giỏ trống lại.

## Next action
- XONG feature Tài khoản. Phase 2 chỉ còn Pricing engine đầy đủ.

## Vấn đề mở (reviewer Cổng 5)
- [H1] Trang /account thiếu <h1> (quy ước SEO/a11y) → đổi tên user thành <h1>.
- [M2] Thumbnail lấy ảnh product-level, bỏ ảnh theo variant → ưu tiên ảnh VariantId khớp.
- [L2] VariantSlug nạp nhưng view không dùng → link tên SP về PDP.
- [M1] x-cloak tab Thông tin (noindex → chấp nhận). [L1] OrderBy Id thay CreatedAt (chấp nhận, Id đơn điệu).
- Điểm tốt: customerId từ identity (không IDOR); [Authorize]; guest null tự loại; giá snapshot; 1-query không N+1;
  XSS an toàn (@ encode); antiforgery logout.

## Contract (Cổng 2) — chốt
- KHÔNG đổi schema → KHÔNG migration.
- App: Features/Orders — OrderStatusInfo (thuần: OrderStatus → nhãn VN + StepIndex 0..3, -1 nếu huỷ/hoàn) +
  OrderHistoryService.GetMyOrdersAsync(customerId) → OrderSummaryDto[] + Dtos (OrderSummaryDto, OrderLineView).
  Lịch sử dùng OrderItem.UnitPrice snapshot.
- Web: AccountController.Index nạp đơn (ICurrentUser.UserId) + user (UserManager.GetUserAsync) → AccountVm.
  Index.cshtml viết lại thành dashboard (tabs Alpine). Đăng ký DI OrderHistoryService.
- Docs: api.md (/account dashboard).
- [ ] 2. Contract        — TODO
- [ ] 3. Test (red)      — TODO
- [ ] 4. Implement       — TODO
- [ ] 5. Review subagent — TODO
- [ ] 6. Review tay       — TODO
- [ ] 7. Test (green)    — TODO
- [ ] 8. Commit          — TODO

## Thiết kế (Cổng 0) — bundle ShopDunk (dòng 507-573)
- Dashboard khi đã đăng nhập: avatar(chữ cái)+tên+SĐT+Đăng xuất; tabs Đơn hàng / Yêu thích / Thông tin.
  - Đơn hàng: list — mã đơn, ngày, trạng thái (ORDER_STEPS: Đã đặt/Đang xử lý/Đang giao/Hoàn thành),
    item (tên/variant/SL/giá dòng), tổng tiền.
  - Yêu thích: grid SP — CẦN entity Wishlist (chưa có trong domain).
  - Thông tin: tên/SĐT/hạng thành viên (membership là DEMO — ta chưa có).

## Quyết định / dữ liệu sẵn có
- Order/OrderItem có sẵn; query theo CustomerId. Lịch sử đơn dùng **OrderItem.UnitPrice (snapshot)** — KHÔNG
  IPricingService (đơn cũ hiển thị giá đã trả).
- /account hiện là landing tối thiểu (từ feature Auth) → feature này mở rộng thành dashboard. [Authorize] sẵn.
- Không đổi schema (nếu bỏ Wishlist).

## Vấn đề mở (chờ chốt Cổng 0)
- Wishlist: làm luôn (cần entity Wishlist + toggle ở PDP/card → MIGRATION + nhiều việc) hay DỜI sang feature riêng?
- Info tab: chỉ Họ tên + Email (bỏ "hạng thành viên" vì chưa có membership) — đề xuất.

## Next action
- Chốt phạm vi tab (wishlist in/out) → Cổng 1 Spec.
