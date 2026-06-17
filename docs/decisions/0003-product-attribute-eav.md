# 0003 — Bỏ ProductVariant, biến thể = Product con + ProductAttribute (EAV)

- **Trạng thái:** Proposed (chờ duyệt)
- **Ngày:** 2026-06-17
- **Liên quan:** [0001-clean-architecture](0001-clean-architecture.md), `docs/data-model.md` §1 Catalog

## Bối cảnh

Mô hình cũ: `Product` (model) 1–N `ProductVariant` (SKU = tổ hợp dung lượng×màu). Biến thể giữ
`Sku, Slug, Storage, Color, BasePrice, CompareAtPrice, Status`. **5–7 bảng FK tới `ProductVariant`**
(`OrderItem, CartItem, Review, Inventory, StockNotification, BundleItem, ProductImage`).

Yêu cầu nghiệp vụ mới: linh hoạt thuộc tính (không chỉ dung lượng/màu — còn RAM, kích thước, chất liệu…)
mà không phải thêm cột mỗi lần. Đồng thời gộp biến thể về chung 1 bảng với sản phẩm.

## Quyết định

1. **Bỏ hẳn entity/bảng `ProductVariant`.**
2. **Biến thể = dòng trong `Product`** qua self-FK `ProductParentId`:
   - `ProductParentId = null` → **sản phẩm cha** (model, dùng để gom + hiển thị chung, **không bán trực tiếp**).
   - `ProductParentId = <id cha>` → **biến thể con** (đơn vị **bán thực sự**: có giá, SKU, tồn kho).
3. **`Product` thêm cột** (chuyển từ variant — quyết định duyệt: giá/SKU/status là cột typed, KHÔNG để EAV):
   - `ProductParentId long?`, `Sku string? (unique khi NOT NULL)`, `BasePrice decimal(18,2)`,
     `CompareAtPrice decimal?(18,2)`, `Status` (enum, đổi tên `VariantStatus`→`ProductStatus`).
   - Giữ nguyên: `CategoryId, Name, Slug (unique), Tagline, Description, Brand, SpecsJson`.
   - **Bỏ** ý định nhét `Storage/Color` thành cột → chúng thành `ProductAttribute`.
4. **`Attribute`** (master) — định nghĩa loại thuộc tính: `Id, Name (unique), SortOrder?`. Vd "Dung lượng", "Màu", "RAM".
5. **`ProductAttribute`** (EAV value) — đúng ảnh: `Id (PK), AttributeId (FK→Attribute), ProductId (FK→Product),
   Value nvarchar(500), CreatedDate`. Unique `(ProductId, AttributeId)`. Lưu giá trị thuộc tính cho từng
   product/biến thể (vd biến thể X: {Dung lượng:"256GB"}, {Màu:"Đen"}).
6. **Repoint mọi FK `VariantId` → `ProductId`** (trỏ tới Product con/bán):
   - `OrderItem.VariantId → ProductId`, `CartItem.VariantId → ProductId`, `Review.VariantId → ProductId`,
     `Inventory.VariantId → ProductId`, `StockNotification.VariantId → ProductId`,
     `BundleItem.AccessoryVariantId → AccessoryProductId`, `ProductImage`: bỏ `VariantId`, chỉ giữ `ProductId`
     (ảnh gắn product cha hoặc con).
7. **Pricing**: `IPricingService.GetEffectivePriceAsync(productId)` đọc `Product.BasePrice/CompareAtPrice`
   của product con. `PricingContext` đổi `(VariantId, ProductId, CategoryId)` → `(ProductId, ParentProductId,
   CategoryId)`. Promotion conditions: `variantIds` → khớp `productId` (con), `productIds` → khớp `parentId`.
8. **Slug/PDP**: mỗi `Product` có `Slug` unique. `SlugResolver` phân giải slug → `Product`. PDP:
   - slug là con → cha = `ProductParentId`, danh sách biến thể = các con cùng cha.
   - slug là cha → chọn con đầu làm mặc định.
   - `VariantOptionDto` dựng từ các Product con + `ProductAttribute` (hiển thị "256GB · Đen").

## Hệ quả

- **Migration phá huỷ** (reset DB + reseed — đã duyệt cho môi trường dev). Mất dữ liệu đơn/giỏ dev hiện có.
- Đụng diện rộng: Domain (3 entity sửa + 2 mới + 1 xoá, repoint 7 FK), Infrastructure (configs, migration, seed),
  Application (Pricing/Catalog/AdminProduct/Cart/Bundle/Checkout/StockNotification/Search + DTO), Web (PDP, admin
  product, cart, slug), ~10 file test.
- **Rủi ro tiền**: giữ nguyên `PriceCalculator` (logic % + cố định + anchor CompareAt) — chỉ đổi nguồn đọc giá từ
  `Product` thay vì `ProductVariant`. Test pricing cũ vẫn áp dụng (PriceCalculator thuần không đổi).
- EAV: query lọc theo thuộc tính (vd "256GB") cần join `ProductAttribute` — chậm hơn cột cứng; chấp nhận vì
  catalog đọc qua cache + quy mô nhỏ.

## Phương án đã loại

- **Giá/Status/SKU để trong ProductAttribute (EAV)**: loại — phá precision decimal, parse string ở hot path
  pricing, khó check unique SKU + query ẩn/hiện. (Đã hỏi & duyệt chọn cột typed.)
- **Giữ data cũ (migration không phá huỷ)**: loại cho dev — phức tạp, dễ sai; dữ liệu chỉ là seed.
