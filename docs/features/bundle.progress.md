# Tiến độ: Bundle "mua kèm phụ kiện" — Phase 3
slug: bundle
Cập nhật: 2026-06-17 01:40
Cổng hiện tại: 2

## Trạng thái cổng
- [x] 0. Thiết kế/UI      — DONE (không có design → tự dựng theo SPEC §5 + tokens)
- [x] 1. Spec            — DONE (duyệt: +UnitPriceOverride migration; checkbox chọn)
- [x] 2. Contract        — DONE (duyệt)
- [x] 3. Test (red)      — DONE (3 BundleMath đỏ)
- [x] 4. Implement       — DONE (build xanh, 73 test pass; migration CartItemUnitPriceOverride CHƯA apply)
- [x] 5. Review subagent — DONE (0 Critical)
- [x] 6. Review tay       — DONE (duyệt: H2 lọc Active; H1 giữ + ghi rõ)
- [x] 7. Test (green)     — DONE (73 test pass)
- [x] 8. Commit           — IN_PROGRESS

## Đã sửa (Cổng 7)
- H2: GetForProductAsync lọc phụ kiện Status==Active. H1: comment chủ đích (giá kèm áp cả dòng, có lợi khách).

## Next action
- CHỜ DUYỆT: apply migration CartItemUnitPriceOverride → smoke-test PDP bundle → /cart giá mua kèm.

## Vấn đề mở (reviewer Cổng 5)
- [H2] GetForProductAsync không lọc phụ kiện Status==Active → widget hiện phụ kiện ngừng bán, lệch giỏ. → thêm filter Active.
- [H1] AddItemAsync khi merge dòng đã có + override: ghi đè giá cả phần đã có (customer-favorable). → giữ + ghi rõ
  chủ đích (Phase 3, có lợi khách, không overcharge) + comment; out-of-scope "áp giá kèm khi thêm lẻ".
- [M3] AddToCartAsync nhiều SaveChanges không atomic (COD/demo chấp nhận, ghi nhận).
- [M4/M5] Widget "main" = giá tạm tính client (chốt ở giỏ/checkout) — chấp nhận.
- Điểm tốt: giá kèm từ DB + validate ∈ bundle (chống chỉnh giá); override→checkout snapshot đúng; clamp saving;
  antiforgery; XSS an toàn (chỉ nhúng số vào Alpine); Clean Architecture/Dependency Rule đạt.

## Contract (Cổng 2) — chốt
- Domain: CartItem +UnitPriceOverride decimal? → MIGRATION CartItemUnitPriceOverride.
- App: BundleService (Features/Bundles): GetForProductAsync(productId)→BundleView? (đọc cho PDP);
  AddToCartAsync(owner, mainVariantId, accessoryVariantIds[]) — load Bundle, validate phụ kiện ∈ bundle,
  add chính (giá thường) + phụ kiện (override = BundleItem.BundlePrice TỪ DB, KHÔNG tin client). BundleMath thuần.
- ICartService.AddItemAsync(+ decimal? unitPriceOverride=null); GetCartAsync: dùng UnitPriceOverride ?? IPricingService.
- ProductDetailDto +BundleView? Bundle; CatalogService.BuildDetailAsync gọi BundleService.GetForProductAsync.
- Web: BundleController POST /bundle/add (antiforgery); widget ở PDP. Seed 1 bundle (iPhone + AirPods).
- Docs: data-model (CartItem.UnitPriceOverride), api.md (/bundle/add).

## Next action
- Chờ duyệt Contract → Cổng 3 test BundleMath (red) → Cổng 4 (+migration).
- [ ] 2. Contract        — TODO
- [ ] 3. Test (red)      — TODO
- [ ] 4. Implement       — TODO
- [ ] 5. Review subagent — TODO
- [ ] 6. Review tay       — TODO
- [ ] 7. Test (green)    — TODO
- [ ] 8. Commit          — TODO

## Thiết kế (Cổng 0)
- Bundle ShopDunk KHÔNG có UI "mua kèm phụ kiện". SPEC §5 mô tả: SP chính + phụ kiện gợi ý (giá niêm yết &
  giá mua kèm) + tổng tiết kiệm + nút "Mua N sản phẩm". → đề xuất tự dựng theo SPEC §5 + tokens + ProductCard/PriceBlock.

## Dữ liệu sẵn có
- Entity Bundle(MainProductId) + BundleItem(BundleId, AccessoryVariantId, BundlePrice) đã có + DbSet.
- Giá qua IPricingService (giá niêm yết phụ kiện) so với BundlePrice (giá mua kèm) → tính tiết kiệm.

## Next action
- Chốt Cổng 0 (tự dựng theo SPEC §5 hay có design riêng) → Cổng 1 Spec.
