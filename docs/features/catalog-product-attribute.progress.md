# Tiến độ: Catalog — bỏ ProductVariant, dùng Product con + ProductAttribute
slug: catalog-product-attribute
Cập nhật: 2026-06-17 09:00
Cổng hiện tại: 5/6 (Implement xong, build+test xanh; chờ review + duyệt chạy migration reset)

## Đã implement (Cổng 4) — build 0 warning, 147 test xanh
- Domain: +Attribute, +ProductAttribute, Product(+ProductParentId/Sku/BasePrice/CompareAtPrice/Status), xoá ProductVariant,
  enum VariantStatus→ProductStatus, repoint FK VariantId→ProductId (OrderItem/CartItem/Review/Inventory/StockNotification),
  AccessoryVariantId→AccessoryProductId, ProductImage bỏ VariantId.
- Infra: CatalogConfigurations (Product self-FK, Sku filtered unique, Attribute/ProductAttribute), SalesConfig CartItem index,
  DbContext +Attributes/+ProductAttributes -ProductVariants, seed lại (cha+con+attribute), migration reset Initial mới.
- Application: PricingEngine (đọc giá Product con), PricingContext(ProductId,ParentProductId,CategoryId), PromotionConditions
  (variantIds→con, productIds→cha), CatalogService (PDP cha/con + attribute label), AdminProductService (cha+con, attr Dung lượng/Màu),
  Cart/Bundle/Checkout/Order/StockNotification/Search; DTO giữ tên VariantId/VariantSlug/VariantText (= Product con).
- Web: SlugResolver→Products, PDP (Label thay Storage/Color), admin product views enum, ProductViewModels enum.
- Tests: cập nhật 6 file (Pricing/OrderFactory/AdminProduct/Cart/Checkout/StockNotification) sang model mới.

## Trạng thái cổng
- [x] 0. Thiết kế/UI      — DONE (mô hình do user chỉ định + ảnh ProductAttribute; PDP/admin tái dùng layout sẵn)
- [x] 1. Spec            — DONE (4 câu hỏi đã chốt: Attribute master, Product+ProductParentId, giá/Sku/Status thành cột typed, reset DB)
- [ ] 2. Contract        — IN_PROGRESS (ADR 0003 đã viết; chờ DUYỆT trước khi code — có migration phá huỷ + đụng pricing)
- [ ] 3. Test (đỏ)       — TODO
- [ ] 4. Implement       — TODO
- [ ] 5. Review subagent — TODO
- [ ] 6. Review tay       — TODO (STOP — tiền/kho/quyền)
- [ ] 7. Test (xanh)     — TODO
- [ ] 8. Commit          — TODO

## Contract chốt (xem ADR docs/decisions/0003-product-attribute-eav.md)
- Domain: + `Attribute`, `ProductAttribute`; sửa `Product` (+ProductParentId, +Sku, +BasePrice, +CompareAtPrice, +Status);
  xoá `ProductVariant`; đổi `VariantStatus`→`ProductStatus`. Repoint FK VariantId→ProductId ở 7 bảng.
- Application: đổi `IPricingService` ký hiệu variantId→productId; `PricingContext`; CatalogService (PDP theo Product con/cha
  + attribute); AdminProductService (CRUD product con + attribute); CartService/BundleService/CheckoutService/OrderFactory/
  StockNotificationService/Search đổi VariantId→ProductId. DTO tương ứng.
- Web: SlugResolver (Product), PDP view, admin product views (quản lý biến thể = product con + form attribute), cart view.
- Infra: configs (Attribute/ProductAttribute, Product self-FK, unique Sku/Slug), migration reset, seed lại theo model mới.

## Kế hoạch triển khai (thứ tự, sau khi duyệt Contract)
1. **Domain**: entity Attribute, ProductAttribute; sửa Product; xoá ProductVariant; đổi enum; repoint FK (đổi tên field).
2. **Infrastructure**: IEntityTypeConfiguration mới + sửa; IApplicationDbContext/AppDbContext (DbSet Attributes, ProductAttributes;
   bỏ ProductVariants). Migration: gộp về 1 migration reset (dev) hoặc add migration mới.
3. **Application**: Pricing trước (đọc giá từ Product) → Catalog (PDP/cards/slug) → AdminProduct → Cart/Bundle/Checkout/Order/
   StockNotification/Search. Cập nhật DTO.
4. **Web**: SlugResolver, PDP, admin product (Index/Create/Edit + form attribute), cart partials.
5. **Seed**: dựng cha + con + ProductAttribute + Attribute master; inventory/bundle/stock theo productId con.
6. **Tests (Cổng 3 trước Cổng 4)**: viết/đỏ cho phần logic thuần mới (vd map attribute, chọn biến thể mặc định); sửa ~10 test cũ
   (VariantId→ProductId; service tests). PriceCalculator thuần giữ nguyên.

## Quyết định chính (đã duyệt qua hỏi đáp)
- Attribute master: CÓ. Product +ProductParentId. Giá/Sku/Status = cột typed trên Product (không EAV). Reset DB+reseed. Bootstrap đã commit (e8d69b3).

## Vấn đề mở / cần lưu ý
- Product cha có bán trực tiếp không? → ĐỀ XUẤT: cha chỉ gom, con mới bán (giữ giống mô hình cũ: PDP cha → chọn con đầu).
- Promotion conditions JSON: `variantIds`→productId con, `productIds`→parentId. Cần rà seed/promotion mẫu.
- Unique Sku: chỉ áp cho con (cha có thể null Sku). Slug unique toàn bộ Product.

## Review Cổng 5 (code-reviewer) + xử lý
- [SỬA] C1 (translatability): bỏ ternary trên 2 collection-nav (BundleService/OrderHistoryService) → dùng `Parent!.Images` (dịch được). Còn lại scalar ternary giữ.
- [SỬA] C2 (data-loss): FK OrderItem/Review/CartItem/BundleItem → Product nay OnDelete=Restrict (không cascade xoá lịch sử). Migration regenerate.
- [SỬA] M2: chặn thêm giỏ / theo dõi kho cho model cha — yêu cầu ProductParentId != null (chỉ con bán).
- [SỬA] m3: AdminProductService.UpdateAsync invalidate cache giá cho biến thể đổi giá (IPricingService).
- [GHI NHẬN] M1 (ngữ nghĩa promo productIds=cha / variantIds=con) — đã ghi ADR; cân nhắc validate ở admin promo (feature sau).
- [GHI NHẬN] M3 (get-or-create attribute racy) — chấp nhận với dev 1 admin; seed tạo sẵn Dung lượng/Màu.

## Cổng 6/7 — DONE
- Reset DB thật (drop schema public + migrate Initial + seed) — drop DB Supabase không được nên dùng DROP SCHEMA.
- Smoke-test Postgres thật OK: home/category/search/suggest, PDP (nhãn EAV "256GB · Titan", giá đúng), bundle (Parent!.Images
  dịch được — KHÔNG lỗi translate), add-to-cart + cart (nhãn EAV + ảnh model), checkout→order SD000002, admin products
  list (6 model) + edit + AJAX add-variant/toggle, admin order detail (nhãn EAV). → C1 xác nhận dịch được trên Postgres.
- data-model.md cập nhật (Products self-ref, Attributes, ProductAttributes, FK Restrict, bỏ ProductVariants, enum ProductStatus).

## Next action
- Cổng 8: commit (không push). Branch hiện tại feat/phase1-home-productcard.
