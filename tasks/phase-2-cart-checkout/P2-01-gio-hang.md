# P2-01 Giỏ hàng (Cart)

- **Phase:** 2
- **Tính năng SPEC:** §4.1 (Giỏ hàng + empty state), §6 UC-01, §9 (EmptyState)
- **Trạng thái:** doing

## Mục tiêu
Khách (vãng lai theo session hoặc đã đăng nhập) thêm sản phẩm vào giỏ, xem/sửa số lượng, xóa, thấy
tóm tắt đơn (tạm tính + tổng) và đi tiếp tới checkout; giỏ trống có empty state.

## Phạm vi (in-scope)
- [ ] Thêm vào giỏ từ PDP (nút "MUA NGAY" → giỏ, "Trả góp…" để Phase sau) theo `variantId` + qty.
- [ ] Trang `/cart` server-render: danh sách item + tóm tắt + empty state (đúng thiết kế).
- [ ] Tăng/giảm số lượng (stepper) và xóa item — cập nhật qua **htmx** (partial), không reload cả trang.
- [ ] Badge số lượng (`cartCount`) trên icon giỏ ở header (`_Layout`).
- [ ] Giỏ guest gắn `SessionId` (cookie/session); user đăng nhập gắn `CustomerId`.
- [ ] **Merge** giỏ guest vào giỏ user khi đăng nhập (gộp theo variant, cộng qty).
- [ ] Đơn giá item = giá hiệu lực qua `IPricingService` (sau khuyến mãi); tóm tắt: tạm tính = Σ(đơn giá×qty), phí ship = "Miễn phí" (Phase 2), tổng = tạm tính.

## Ngoài phạm vi (out-of-scope)
- Checkout / đặt hàng (P2-02), thanh toán.
- Áp **mã giảm giá** (coupon): UI ô "Mã giảm giá"+"Áp dụng" render tĩnh, **chưa xử lý logic** (để Phase promotions). Nút disabled + tooltip "Sắp có".
- Chặn theo **tồn kho** (trừ kho ở Order/checkout Phase sau). Cart không kiểm tồn.
- Trả góp / thu cũ đổi mới.

## File sẽ đụng
- `src/HtMobile.Application/Common/Interfaces/ICartService.cs` (mới) — hợp đồng thao tác giỏ.
- `src/HtMobile.Application/Features/Cart/CartService.cs` + `Dtos/CartDtos.cs` (mới).
- `src/HtMobile.Infrastructure/Persistence/Configurations/SalesConfigurations.cs` (nếu chưa cấu hình Cart/CartItem).
- `src/HtMobile.Web/Areas/Storefront/Controllers/CartController.cs` (đã có skeleton — bổ sung action).
- `src/HtMobile.Web/Areas/Storefront/Views/Cart/Index.cshtml` + partial `_CartSummary`/`_CartItems`.
- `src/HtMobile.Web/Views/Shared/_Layout.cshtml` (badge cartCount).
- `src/HtMobile.Web/Areas/Storefront/Views/Catalog/ProductDetail.cshtml` (nút MUA NGAY → POST /cart/items).
- `tests/HtMobile.Application.UnitTests/Cart/...` (logic tóm tắt + merge + qty).

## Phụ thuộc
- Entity `Cart`/`CartItem` (đã có), DbSet `Carts`/`CartItems` (đã có), `IPricingService` (đã có).
- Định danh giỏ guest: cần cơ chế lấy/định danh `SessionId` ở Web (đã `AddSession`).

## Tiêu chí done
- [ ] Build xanh, không vi phạm Dependency Rule (Application không ref Npgsql; DB qua `IApplicationDbContext`).
- [ ] Thêm SP từ PDP → badge tăng; `/cart` hiện đúng item + tổng; inc/dec/xóa cập nhật tổng tức thì (htmx).
- [ ] Giỏ trống → empty state đúng thiết kế.
- [ ] Đăng nhập khi đang có giỏ guest → giỏ được merge.
- [ ] Unit test: tính tóm tắt (Σ đơn giá×qty), gộp item trùng variant, giảm qty về 0 thì xóa.

## Cách verify
- Chạy app (Redis "memory" dev OK), mở PDP → "MUA NGAY" → `/cart`; chỉnh qty/xóa; kiểm badge + tổng.
- `dotnet test` cho phần logic giỏ.
