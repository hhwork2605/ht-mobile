# P5-01 Admin — Quản lý đơn hàng

- **Phase:** 5
- **Tính năng SPEC:** §4.3 (Admin: quản lý đơn hàng & trạng thái), §7 (Order)
- **Trạng thái:** doing

## Mục tiêu
Admin xem danh sách đơn (lọc theo trạng thái), mở chi tiết đơn, và **đổi trạng thái** theo luồng hợp lệ.
Toàn bộ trong Area Admin, chỉ role Admin truy cập.

## Phạm vi (in-scope)
- [ ] **Layout admin riêng** (`Areas/Admin/Views/Shared/_Layout`): sidebar (Bảng điều khiển, Đơn hàng — active;
      mục khác là link/placeholder) + header (tiêu đề, email admin, Đăng xuất, link ra storefront). Design tokens.
- [ ] `GET /admin/orders?status=` : tab lọc theo trạng thái (count mỗi nhóm) + bảng (Mã đơn `SD{Id:D6}`, khách
      (Shipment.Address), Sản phẩm (tóm tắt item), Tổng tiền, Thanh toán, Trạng thái badge, Ngày). Mới nhất trước.
- [ ] `GET /admin/orders/{id}` : chi tiết (thông tin nhận hàng/Shipment, danh sách OrderItem + giá, tổng, thanh toán,
      trạng thái hiện tại) + form đổi trạng thái (chỉ hiện các trạng thái kế tiếp hợp lệ).
- [ ] `POST /admin/orders/{id}/status` (antiforgery): đổi trạng thái — **validate chuyển hợp lệ** (server), lưu.
- [ ] Logic thuần `OrderStatusTransition`: `CanTransition(from,to)` + `AllowedNext(from)`. Có test.
- [ ] Tất cả `[Authorize(Roles = Admin)]`.

## Ngoài phạm vi (out-of-scope)
- Admin CRUD sản phẩm/khuyến mãi/người dùng/CMS, báo cáo — feature riêng (P5-02+).
- Sửa item/tiền trong đơn, hoàn tiền thực, in hoá đơn, gửi email cập nhật trạng thái.
- Tồn kho theo cửa hàng; store switcher (header chỉ tĩnh).
- Lọc nâng cao/tìm kiếm/sắp xếp cột (chỉ lọc theo trạng thái + mới nhất trước).

## File sẽ đụng
- `src/HtMobile.Application/Features/Orders/OrderStatusTransition.cs` (thuần) + `AdminOrderService.cs` + Dtos.
- `src/HtMobile.Web/Areas/Admin/Views/Shared/_Layout.cshtml` (+ `_ViewStart` trỏ layout này).
- `src/HtMobile.Web/Areas/Admin/Controllers/OrdersController.cs` + Views (Index, Details).
- `tests/HtMobile.Application.UnitTests/Orders/OrderStatusTransitionTests.cs`.

## Phụ thuộc
- Order/OrderItem/Shipment/Payment (đã có), OrderStatus enum, OrderStatusInfo (P2-04, tái dùng nhãn), Identity Admin role.
- KHÔNG entity/DB/migration mới.

## Tiêu chí done
- [ ] Build xanh, Dependency Rule OK (DB qua IApplicationDbContext; transition thuần).
- [ ] /admin/orders hiện đơn + lọc theo trạng thái (count đúng); /admin/orders/{id} chi tiết đầy đủ.
- [ ] Đổi trạng thái hợp lệ → lưu + phản ánh; chuyển KHÔNG hợp lệ (vd Delivered→Pending) → từ chối.
- [ ] Non-admin / chưa đăng nhập → chặn (redirect login / 403).
- [ ] Unit test OrderStatusTransition (luồng hợp lệ + chặn lùi + terminal Cancelled/Refunded).

## Cách verify
- Đăng nhập admin@htmobile.local → /admin/orders → thấy đơn đã đặt (P2-02) → mở chi tiết → đổi Pending→Confirmed.
- `dotnet test` cho OrderStatusTransition.
