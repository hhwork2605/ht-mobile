# P3-01 Bundle "mua kèm phụ kiện"

- **Phase:** 3
- **Tính năng SPEC:** §4.2 (bundle), §5 (widget mua kèm), §6 UC-02
- **Trạng thái:** doing

## Mục tiêu
Trên PDP, nếu sản phẩm có combo, hiện khối **"Mua kèm phụ kiện"**: SP chính + phụ kiện gợi ý với
**giá mua kèm** (rẻ hơn giá niêm yết) + **tổng tiết kiệm**; nút thêm SP chính + phụ kiện đã chọn vào giỏ,
phụ kiện tính **giá mua kèm**.

## Phạm vi (in-scope)
- [ ] PDP: nạp `Bundle` của sản phẩm (nếu có) → khối widget dưới phần mua hàng. Mỗi phụ kiện: tên, ảnh,
      giá niêm yết (gạch ngang) + giá mua kèm + tiết kiệm; checkbox chọn; tổng tiền + **tổng tiết kiệm**; nút "Mua N sản phẩm".
- [ ] Giá niêm yết phụ kiện lấy qua `IPricingService`; giá mua kèm = `BundleItem.BundlePrice`. Tiết kiệm = niêm yết − mua kèm.
- [ ] Logic thuần `BundleMath`: tổng giá mua kèm + tổng tiết kiệm từ danh sách dòng (có test).
- [ ] `POST /bundle/add` (antiforgery): thêm variant chính (giá thường) + các phụ kiện đã chọn (giá mua kèm) vào giỏ → về /cart.
- [ ] **Giá mua kèm có hiệu lực trong giỏ**: thêm `CartItem.UnitPriceOverride decimal?` (MIGRATION); giỏ/checkout dùng
      override nếu có, ngược lại giá hiệu lực `IPricingService`.

## Ngoài phạm vi (out-of-scope)
- Quản lý/định nghĩa bundle ở Admin (Phase 5) — Phase 3 seed mẫu để demo/test.
- Bundle lồng nhau, điều kiện số lượng, bundle như 1 promotion.
- Áp lại giá mua kèm khi phụ kiện được thêm lẻ (không qua bundle) — chỉ khi mua qua widget.

## File sẽ đụng
- `src/HtMobile.Domain/Entities/Sales/CartItem.cs` (+`UnitPriceOverride decimal?`).
- `src/HtMobile.Infrastructure/Persistence/Configurations/SalesConfigurations.cs` + migration.
- `src/HtMobile.Application/Features/Catalog/CatalogService.cs` + `ProductDetailDto` (+Bundle).
- `src/HtMobile.Application/Features/Bundles/BundleMath.cs` (thuần) + Dtos.
- `src/HtMobile.Application/Features/Cart/CartService.cs` (AddItem nhận override; GetCartAsync ưu tiên override).
- `src/HtMobile.Web/Areas/Storefront/Controllers/BundleController.cs` (hoặc thêm action) + view widget ở PDP.
- `tests/HtMobile.Application.UnitTests/Bundles/BundleMathTests.cs`.

## Phụ thuộc
- Bundle/BundleItem (đã có), Cart (P2-01), IPricingService, Checkout (OrderFactory dùng cart line UnitPrice).

## Tiêu chí done
- [ ] Build xanh, Dependency Rule OK; tiền qua IPricingService (niêm yết) + BundlePrice (mua kèm).
- [ ] PDP có bundle → hiện widget đúng giá/tiết kiệm; sản phẩm không có bundle → không hiện.
- [ ] "Mua N" thêm chính + phụ kiện đã chọn; trong giỏ phụ kiện hiện **giá mua kèm** (override); tổng đúng.
- [ ] Unit test BundleMath (tổng + tiết kiệm, bỏ chọn).
- [ ] Migration `CartItemUnitPriceOverride` (tạo, chờ duyệt apply).

## Cách verify
- Seed 1 bundle (iPhone + AirPods giá mua kèm) → PDP iPhone thấy widget → "Mua kèm" → /cart thấy AirPods giá ưu đãi.
- `dotnet test` cho BundleMath.
