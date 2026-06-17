# P3-04 Theo dõi hàng về (StockNotification)

- **Phase:** 3
- **Tính năng SPEC:** §4.1 (theo dõi để biết khi có hàng), §6 UC-05, §7 (StockNotification)
- **Trạng thái:** doing

## Mục tiêu
Sản phẩm (variant) hết hàng → khách đăng ký email/SĐT để nhận thông báo khi có hàng; hệ thống lưu
`StockNotification`. Thuần ghi nhận đăng ký (chưa gửi thông báo thật khi nhập kho).

## Phạm vi (in-scope)
- [ ] PDP: khi variant đang chọn **hết hàng** (`!InStock`) → thay nút "MUA NGAY" bằng form **"Theo dõi để biết khi có hàng"**
      (1 ô email/SĐT + nút Đăng ký), gửi qua **htmx** → trả partial xác nhận "đã đăng ký".
- [ ] Logic thuần `ContactValidator.IsValid(contact)`: hợp lệ nếu là email HOẶC SĐT VN. Có test.
- [ ] `POST /stock-notify` (antiforgery): validate contact + variant tồn tại + **đang hết hàng**; **dedupe** (variant+contact
      đã đăng ký & chưa notified → coi như thành công, không tạo trùng); tạo `StockNotification` (Notified=false).
- [ ] Seed demo: đảm bảo có ≥1 variant `OutOfStock` (set 1 variant định sẵn nếu chưa có) để xem/test.

## Ngoài phạm vi (out-of-scope)
- Gửi email/SMS thật khi hàng về (cần job + IEmailSender thật) → Phase sau.
- Quản lý danh sách theo dõi ở Admin (Phase 5). Huỷ theo dõi.
- Tự suy "hết hàng" theo tồn kho thực (Inventory) — Phase 3 dựa `VariantStatus` (Active = còn, khác = hết).

## File sẽ đụng
- `src/HtMobile.Application/Features/StockNotifications/ContactValidator.cs` (thuần) + `StockNotificationService.cs`.
- `src/HtMobile.Web/Areas/Storefront/Controllers/StockNotificationController.cs` + partial `_StockNotifyForm`/`_StockNotifyDone`.
- `src/HtMobile.Web/Areas/Storefront/Views/Catalog/ProductDetail.cshtml` (nhánh hết hàng).
- `src/HtMobile.Infrastructure/Persistence/Seed/DbInitializer.cs` (đảm bảo 1 variant OutOfStock).
- `tests/HtMobile.Application.UnitTests/StockNotifications/ContactValidatorTests.cs`.

## Phụ thuộc
- `StockNotification` + DbSet (đã có), `VariantStatus` (đã có). KHÔNG entity/migration mới.

## Tiêu chí done
- [ ] Build xanh, Dependency Rule OK (DB qua IApplicationDbContext; validator thuần).
- [ ] PDP variant hết hàng → hiện form theo dõi (không có nút MUA NGAY); đăng ký → partial xác nhận; DB có StockNotification.
- [ ] Contact không hợp lệ → báo lỗi, không tạo. Variant còn hàng → không tạo (báo còn hàng/mua được).
- [ ] Đăng ký trùng (variant+contact) → idempotent (không tạo bản ghi thứ 2).
- [ ] Unit test: ContactValidator (email/SĐT hợp lệ + chuỗi bậy + rỗng).

## Cách verify
- PDP variant OutOfStock → nhập email → Đăng ký → "đã đăng ký"; DB có StockNotification; đăng ký lại không nhân đôi.
- `dotnet test` cho ContactValidator.
