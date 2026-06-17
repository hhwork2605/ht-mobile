# Tiến độ: Checkout (đặt hàng) — Phase 2
slug: checkout
Cập nhật: 2026-06-17 00:10
Cổng hiện tại: 2

## Trạng thái cổng
- [x] 0. Thiết kế/UI      — DONE (guest checkout; CHỈ COD; tái dùng card ShopDunk)
- [x] 1. Spec            — DONE (duyệt; guest không xem lại đơn — OK)
- [x] 2. Contract        — DONE (duyệt; migration OrderCustomerNullable)
- [x] 3. Test (red)      — DONE (3 test OrderFactory đỏ)
- [x] 4. Implement       — DONE (build xanh, 39 test pass; migration tạo, CHƯA apply)
- [x] 5. Review subagent — DONE (0 Critical; tiền đúng)
- [x] 6. Review tay       — DONE (duyệt fix H1/H2/M1/M2/M3/L4)
- [x] 7. Test (green)     — DONE (41 test pass)
- [x] 8. Commit           — IN_PROGRESS

## Vấn đề mở (reviewer Cổng 5)
- [H1/H2] Double-submit tạo 2 đơn + "tạo đơn/xoá giỏ" 2 SaveChanges (không atomic). → gộp xoá giỏ vào cùng
  SaveChanges với Order (atomic) + disable nút submit client-side.
- [M1] ShipMethod không validate → [RegularExpression store|delivery]. [M3] Phone lỏng → regex VN.
- [M2] FullName/Phone/Address thiếu [StringLength] → thêm (SPEC §13). XSS: success không render địa chỉ (an toàn).
- [L2] POST đọc giỏ 2 lần (định giá 2 lần) — tối ưu sau. [L3] Payment.Provider="COD" hơi lệch nghĩa — chấp nhận.
- [L4] Chưa test CheckoutService (chỉ OrderFactory) → thêm integration test (giỏ rỗng→null; đặt xong giỏ trống).
- Điểm tốt: tiền đúng (Total=Σ, snapshot qua IPricingService, Payment.Amount=Total); antiforgery; guest null OK;
  Clean Architecture đạt; migration chỉ alter nullable; noindex.
- [ ] 2. Contract        — TODO
- [ ] 3. Test (red)      — TODO
- [ ] 4. Implement       — TODO
- [ ] 5. Review subagent — TODO
- [ ] 6. Review tay       — TODO
- [ ] 7. Test (green)    — TODO
- [ ] 8. Commit          — TODO

## Thiết kế (Cổng 0) — bundle ShopDunk (dòng 417-482)
- Form thanh toán: Thông tin nhận hàng (tên/SĐT/địa chỉ) + Phương thức giao (store/delivery, đều miễn phí)
  + Phương thức thanh toán (cod/bank/card/install) + card tóm tắt đơn (item, tạm tính, ship miễn phí, tổng, nút "Đặt hàng").
- Màn "Đặt hàng thành công": tick xanh, h1, nút "Xem đơn hàng" / "Về trang chủ".
- SHIP=[store, delivery]; PAY=[cod, bank, card, install]; placeOrder: tạo order từ giỏ → xoá giỏ → success.

## Quyết định / dữ liệu sẵn có
- Order/OrderItem/Shipment(Address string)/Payment đã có. Order.CustomerId KHÔNG nullable → checkout cần đăng nhập
  (hoặc đổi schema cho guest). OrderItem.UnitPrice = snapshot giá hiệu lực lúc đặt.
- KHÔNG đổi schema nếu chọn login-required.

## Vấn đề mở (chờ chốt)
- Truy cập checkout: BẮT BUỘC ĐĂNG NHẬP (đề xuất, không migration) hay cho guest (cần CustomerId nullable → migration)?
- Thanh toán: Phase 2 KHÔNG tích hợp cổng thật (VNPAY ở Phase 3) → đặt hàng tạo Order=Pending; COD chạy, các
  phương thức khác chỉ tạo đơn Pending (note). Inventory: Phase 2 KHÔNG trừ kho (chưa quản kho) — ghi nhận.

## Contract (Cổng 2) — chốt
- Domain: Order.CustomerId → long? (guest=null). Order không FK cứng tới AspNetUsers (chỉ alter nullable) → MIGRATION.
- App: Features/Checkout — OrderFactory (thuần: build Order từ priced lines, throw/null nếu rỗng) + CheckoutService
  (đọc giỏ qua CartService/IPricingService, tạo Order/OrderItem/Shipment/Payment, xoá giỏ) + Dtos.
- Web: CheckoutController GET/POST /checkout + CheckoutVm + Views Index/Success. Bật nút checkout ở _CartBody.
- Mã đơn hiển thị = SD{Id:D6} (không thêm cột). Giá dòng = IPricingService snapshot vào OrderItem.UnitPrice.
- Docs: data-model (Order.CustomerId nullable), api.md (mục Checkout).

## Đã sửa (Cổng 7)
- H1/H2: gộp tạo đơn + xoá giỏ vào 1 SaveChanges (atomic, FindOwnerCartAsync) + disable nút submit (Alpine).
  Bỏ ICartService.ClearAsync (không còn dùng).
- M1 ShipMethod regex; M2 StringLength tên/SĐT/địa chỉ; M3 Phone regex VN.
- L4: +2 integration test CheckoutService (giỏ rỗng→null; đặt xong snapshot giá + xoá giỏ).

## Next action
- CHỜ DUYỆT: apply migration OrderCustomerNullable (scripts/migrate.ps1 update) → smoke-test runtime đặt hàng.
