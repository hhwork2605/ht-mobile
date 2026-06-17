# P2-02 Checkout (đặt hàng)

- **Phase:** 2
- **Tính năng SPEC:** §4.1 (Checkout), §6 UC-01, §7 (Order/OrderItem/Shipment/Payment)
- **Trạng thái:** doing

## Mục tiêu
Khách (đăng nhập HOẶC vãng lai) đặt hàng từ giỏ: nhập thông tin nhận hàng + chọn phương thức giao,
thanh toán **COD**, tạo đơn (Order Pending) + snapshot giá, xoá giỏ, hiện màn "Đặt hàng thành công".

## Phạm vi (in-scope)
- [ ] `/checkout` GET: form (tên/SĐT/địa chỉ — prefill từ user nếu đăng nhập) + phương thức giao (store/delivery)
      + tóm tắt đơn (item, tạm tính, ship miễn phí, tổng). Giỏ rỗng → chuyển `/cart`.
- [ ] `/checkout` POST: tạo `Order` (CustomerId = user id nếu đăng nhập, else **null** — guest), `OrderItem`
      (snapshot **giá hiệu lực qua IPricingService**), `Total`, `PaymentMethod="COD"`, `Status=Pending`;
      `Shipment` (Address = "tên · SĐT · địa chỉ", Status=Pending); `Payment` (COD, Pending, Amount=Total). Xoá giỏ.
- [ ] Màn "Đặt hàng thành công": mã đơn + nút Về trang chủ / Tiếp tục mua sắm.
- [ ] Guest checkout: `Order.CustomerId` đổi **nullable** (MIGRATION).
- [ ] Thanh toán **chỉ COD** (ẩn bank/card/trả góp — để Phase 3 + cổng thật).

## Ngoài phạm vi (out-of-scope)
- Cổng thanh toán thật (VNPAY/ZaloPay), bank/card/trả góp → Phase 3.
- **Trừ tồn kho** → chưa quản kho ở Phase 2 (ghi nhận; làm khi có Inventory).
- Trang tra cứu/lịch sử đơn (dashboard "Tài khoản" — feature riêng). Guest không có chỗ xem lại đơn (chỉ thấy mã ở màn success).
- Mã giảm giá / phí ship động (ship = miễn phí Phase 2).

## File sẽ đụng
- `src/HtMobile.Domain/Entities/Sales/Order.cs` (CustomerId → `long?`).
- `src/HtMobile.Infrastructure/Persistence/Configurations/SalesConfigurations.cs` (cấu hình nếu cần) + migration.
- `src/HtMobile.Application/Features/Checkout/CheckoutService.cs` + `OrderFactory.cs` (logic thuần build order) + Dtos.
- `src/HtMobile.Web/Areas/Storefront/Controllers/CheckoutController.cs` + Views (Index, Success).
- `src/HtMobile.Web/Models/Checkout/CheckoutVm.cs`.
- `src/HtMobile.Web/Areas/Storefront/Views/Cart/_CartBody.cshtml` (bật nút "Tiến hành thanh toán" → /checkout).
- `tests/HtMobile.Application.UnitTests/Checkout/OrderFactoryTests.cs`.

## Phụ thuộc
- Giỏ (P2-01: `ICartService`, `CartOwner`, `CartContext`), `IPricingService`, Order/OrderItem/Shipment/Payment (đã có).

## Tiêu chí done
- [ ] Build xanh, Dependency Rule OK (DB qua IApplicationDbContext; giá qua IPricingService).
- [ ] Đặt hàng từ giỏ → Order Pending + OrderItem snapshot giá + Shipment + Payment COD; giỏ bị xoá; màn success có mã đơn.
- [ ] Giỏ rỗng vào /checkout → chuyển /cart. Tổng đơn = Σ(giá hiệu lực × qty).
- [ ] Guest (chưa đăng nhập) đặt được đơn (CustomerId null); user đăng nhập → CustomerId set.
- [ ] Unit test OrderFactory: tổng đúng, snapshot giá, giỏ rỗng → không tạo đơn.

## Cách verify
- Chạy app: thêm SP → /cart → "Tiến hành thanh toán" → /checkout → nhập info → Đặt hàng → success + mã đơn; /cart trống lại.
- `dotnet test` cho OrderFactory.
