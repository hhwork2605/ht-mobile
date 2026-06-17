# Tiến độ: Admin — Quản lý đơn hàng — Phase 5
slug: admin-orders
Cập nhật: 2026-06-17 05:10
Cổng hiện tại: 1

## Trạng thái cổng
- [x] 0. Thiết kế/UI      — DONE (đọc ShopDunk Admin.dc.html)
- [x] 1. Spec            — DONE (duyệt)
- [x] 2. Contract        — DONE (duyệt; không schema)
- [x] 3. Test (red)      — DONE (OrderStatusTransition đỏ)
- [x] 4. Implement       — DONE (build xanh, 128 test pass)
- [x] 5. Review subagent — DONE (Đạt, 0 Critical/High)
- [x] 6. Review tay       — DONE (duyệt M1/L3/L5 + test)
- [x] 7. Test (green)     — DONE (131 test pass)
- [x] 8. Commit           — IN_PROGRESS

## Đã sửa (Cổng 7)
- M1: header cột "Nhận hàng". L3: header admin hiện email (User.Identity.Name). L5: ItemsSummary cắt "… +N SP".
- +3 integration test AdminOrderService (valid/invalid/notfound).

## Next action
- Smoke-test: login admin → /admin/orders → chi tiết → đổi trạng thái.

## Vấn đề mở (reviewer Cổng 5)
- [M1] Cột "Khách hàng" đang đổ Shipment.Address (Shipment không tách tên/SĐT) → đổi header cột thành "Nhận hàng".
- [L3] Header admin in cứng "Admin" → hiện email thật (User.Identity.Name). [L5] ItemsSummary dài → cắt gọn.
- Thêm integration test AdminOrderService (ChangeStatus valid/invalid/notfound).
- Chấp nhận/ghi nợ: M2 (Total vs ΣLineTotal — chưa có giảm giá/ship cấp đơn), L4 (nav StartsWith), L6 (chưa phân trang).
- Điểm tốt: [Authorize(Roles=Admin)] ở controller; validate transition server-side (kể cả (OrderStatus)999);
  antiforgery; Clean Architecture; noindex; nav disabled không link chết.

## Contract (Cổng 2) — chốt
- KHÔNG entity/DB/migration.
- App Features/Orders:
  - OrderStatusTransition (thuần): CanTransition(from,to) + AllowedNext(from). Bảng: Pending→{Confirmed,Cancelled};
    Confirmed→{Processing,Cancelled}; Processing→{Shipped,Cancelled}; Shipped→{Delivered}; Delivered→{Refunded};
    Cancelled/Refunded→{} (terminal).
  - AdminOrderService: GetListAsync(status?) → rows + counts mỗi trạng thái; GetDetailAsync(id); 
    ChangeStatusAsync(id,newStatus) → {Ok,NotFound,InvalidTransition}.
  - Dtos: AdminOrderRowDto (Id,Code,Recipient,ItemsSummary,Total,PaymentMethod,Status,CreatedAt), AdminOrderDetailDto.
- Web: Admin/Views/Shared/_Layout (sidebar+header) + _ViewStart trỏ vào; OrdersController (Admin) GET list/detail +
  POST status (antiforgery); [Authorize(Roles=Admin)]. Tái dùng OrderStatusInfo cho nhãn/badge.
- Docs: api.md (mục Admin).

## Next action
- Chờ duyệt Contract → Cổng 3 test OrderStatusTransition (red) → Cổng 4.

## Thiết kế (Cổng 0) — ShopDunk Admin
- Shell: sidebar 244px (nhóm nav: Bảng điều khiển/Báo cáo; Sản phẩm/Đơn hàng/Khuyến mãi; Người dùng/CMS; SEO) +
  header 60px (store switcher, ô tìm, chuông, nút "Tạo mới").
- Màn Đơn hàng: card + tab lọc theo trạng thái (có count) + bảng cột: Mã đơn · Khách hàng(tên+SĐT) · Sản phẩm ·
  Tổng tiền · Thanh toán(badge) · Trạng thái(badge) · Ngày · ⋯. Hàng click → chi tiết.
- Badge trạng thái: Chờ xác nhận/Đang giao/Hoàn thành/Đã huỷ. (Admin hiện mượn _Layout storefront → sẽ làm layout admin riêng.)
- [ ] 2. Contract        — TODO
- [ ] 3. Test (red)      — TODO
- [ ] 4. Implement       — TODO
- [ ] 5. Review subagent — TODO
- [ ] 6. Review tay       — TODO
- [ ] 7. Test (green)    — TODO
- [ ] 8. Commit          — TODO

## Quyết định
- Phạm vi Phase 5 lần này = CHỈ Admin Đơn hàng (danh sách + chi tiết + đổi trạng thái). Các mảng khác (Admin SP/KM,
  báo cáo, SEO/perf/security) là feature riêng.
- Thiết kế: link ShopDunk Admin (https://api.anthropic.com/v1/design/h/_39-g2qGdlRcherGwXAmeQ) trả 404 → chờ link/ảnh mới.

## Dữ liệu sẵn có
- Order/OrderItem/Shipment/Payment đã có. OrderStatus enum (Pending/Confirmed/Processing/Shipped/Delivered/Cancelled/Refunded).
- Area Admin có sẵn + [Authorize(Roles=Admin)] ở DashboardController. Seed có admin@htmobile.local / Admin@123456.
- OrderStatusInfo (P2-04) có thể tái dùng cho nhãn trạng thái.

## Next action
- Nhận link/ảnh thiết kế admin → đọc → Cổng 1 Spec. (Nếu đổi ý "không có" → tự dựng theo SPEC §4.3.)
