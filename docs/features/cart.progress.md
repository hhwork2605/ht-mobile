# Tiến độ: Giỏ hàng (Cart) — Phase 2
slug: cart
Cập nhật: 2026-06-16 11:24
Cổng hiện tại: 5

## Trạng thái cổng
- [x] 0. Thiết kế/UI      — DONE
- [x] 1. Spec            — DONE (duyệt: coupon chỉ UI, merge guest→user làm luôn)
- [x] 2. Contract        — DONE (duyệt; KHÔNG đổi schema)
- [x] 3. Test (red)      — DONE (7 test CartMath đỏ)
- [x] 4. Implement       — DONE (build xanh, 19/19 test pass)
- [x] 5. Review subagent — DONE (không Critical; xem Vấn đề mở)
- [x] 6. Review tay       — DONE (duyệt: làm migration + test CartService)
- [x] 7. Test (green)     — DONE (19 unit + 5 integration pass)
- [ ] 8. Commit           — IN_PROGRESS
- [ ] 6. Review tay      — TODO
- [ ] 7. Test (green)    — TODO
- [ ] 8. Commit          — TODO

## Artifact đã tạo
- docs/features/cart.progress.md
- tasks/phase-2-cart-checkout/P2-01-gio-hang.md (spec)

## Quyết định chính
- Phạm vi: CHỈ giỏ hàng (Cart). Checkout/auth/account là feature riêng.
- Thiết kế: handoff bundle Claude Design (ShopDunk Storefront.dc.html). Tokens trùng _Layout sẵn có.
- Entity Cart/CartItem + DbSet Carts/CartItems đã có sẵn trong Domain/IApplicationDbContext.
- Đơn giá lấy qua IPricingService (giá hiệu lực sau KM), KHÔNG tính tay.

## Thiết kế — điểm UI then chốt (Cổng 0)
- /cart: H1 "Giỏ hàng". Empty: icon mờ + "Giỏ hàng của bạn đang trống." + nút "Tiếp tục mua sắm".
- Có hàng: 2 cột. Trái = list card item (thumb 74, tên, variant "Màu · Dung lượng", đơn giá màu sale,
  nút xóa + stepper [− qty +]). Phải = card "Tóm tắt đơn hàng": Tạm tính (N sp), Phí ship "Miễn phí",
  ô "Mã giảm giá"+"Áp dụng", Tổng cộng (lớn, màu sale), nút "Tiến hành thanh toán" → checkout.
- Header: badge cartCount trên icon giỏ.

## Contract (Cổng 2) — chốt
- KHÔNG đổi schema: Cart/CartItem + DbSet đã có (tạo từ migration Initial). → KHÔNG migration.
- Application: ICartService (mới) + CartDtos. CartOwner(long? CustomerId, string? SessionId).
  - GetCartAsync(owner) → CartDto; AddItemAsync(owner, variantId, qty) → count;
    UpdateQtyAsync(owner, itemId, delta) (về 0 thì xoá); RemoveItemAsync; GetCountAsync(owner);
    MergeAsync(sessionId, customerId).
  - Đơn giá mỗi dòng = IPricingService.GetEffectivePriceAsync (KHÔNG tin CartItem.UnitPrice cũ).
- Web: CartController routes GET /cart, POST /cart/items, /cart/items/{id}/inc|dec|remove (htmx → partial _CartBody).
  - Guest cart id = cookie GUID "htm_cart". ViewComponent CartBadge cho header.
  - MergeAsync gọi ở feature auth (P2) — giờ chỉ implement + unit-test, chưa có call site login.
- Docs đã cập nhật: data-model.md (note giá hiển thị qua IPricingService), api.md (mục Giỏ hàng).

## Vấn đề mở (từ reviewer Cổng 5)
- [H1] AddItemAsync không lọc Status==Active (sửa nhanh, không migration).
- [H2] Thiếu unique index (CartId, VariantId) chống trùng dòng → cần config + MIGRATION.
- [H3] Thiếu unique index Carts.CustomerId / SessionId (filtered) → cùng migration H2.
- [M1] Lệch Contract: chưa có ICartService (đang dùng class cụ thể như CatalogService).
- [M3] MergeAsync set UnitPrice=0 cho dòng mới → copy/recompute.
- [M4] MergeAsync không lọc variant đang bán (cùng H1).
- [L2] variantId nên [FromForm]. [C1] mutate khi owner (null,null) nên báo lỗi rõ thay vì coi giỏ rỗng.
- Test: thêm test CartService (ownership, merge, qty→0) — hiện chỉ có CartMath.
- Điểm tốt: ownership chống IDOR đúng; giá qua IPricingService; antiforgery đủ; SEO noindex; Dependency Rule OK.

## Đã sửa theo review (Cổng 7)
- [H1/M4] AddItem + Merge lọc Status==Active. [M3] Merge giữ UnitPrice nguồn (không 0).
- [M1] Thêm ICartService (Common/Interfaces); CartController + CartBadge dùng interface.
- [L2] Add dùng [FromForm]. [H2/H3] SalesConfigurations + migration CartUniqueIndexes (CHƯA apply).
- Test: +5 integration test CartService (EF InMemory).

## Next action
- CHỜ DUYỆT: apply migration CartUniqueIndexes lên DB (scripts/migrate.ps1 update) — side-effect ngoài repo.
- Sau khi apply: chạy app smoke-test luồng giỏ (thêm/sửa/xóa/badge). App dev tự MigrateAsync nên chạy app = apply.
