# Data model — tham chiếu chi tiết entity & cột

> Nguồn gốc: SPEC §7. Tài liệu này mô tả **từng bảng, từng cột** của tầng Domain
> (`src/HtMobile.Domain/Entities`) cùng ràng buộc khai báo ở
> `src/HtMobile.Infrastructure/Persistence/Configurations`. Khi đổi entity/field/enum/migration
> **phải cập nhật file này trong cùng đợt sửa** (xem bảng Living Documentation ở `CLAUDE.md`).

## Quy ước chung (áp cho mọi bảng)

| Khía cạnh | Quy ước |
|---|---|
| Khóa chính | `Id` kiểu `long` (`bigint`), tự tăng (identity). Khai báo ở `BaseEntity`. |
| Tên bảng/cột | **PascalCase** — mặc định EF Core theo tên CLR, **không** áp naming convention. Tên bảng = tên `DbSet` trong `AppDbContext`. |
| Tiền tệ | Mọi cột `decimal` mặc định **precision `(18,2)`** (cấu hình `ConfigureConventions`). |
| JSON | Cột `*Json` dùng kiểu Postgres **`jsonb`**. |
| Enum | Lưu **dạng số `int`** (giá trị enum), không lưu chuỗi. |
| Audit | Entity kế thừa `BaseAuditableEntity` có `CreatedAt` (NOT NULL) + `UpdatedAt` (NULL), **gán tự động** bởi `AuditableEntityInterceptor` dùng **giờ server** (`IDateTime.Now`) khi `SaveChanges`. Entity chỉ kế thừa `BaseEntity` thì **không** có 2 cột này. |
| Chuỗi không khai `HasMaxLength` | Map sang `text` (không giới hạn độ dài). |
| Khách hàng | `Customer` của SPEC = **`ApplicationUser : IdentityUser<long>`** (bảng `AspNetUsers`). Các cột `CustomerId` là FK trỏ tới `AspNetUsers.Id`. |

### Lớp cơ sở

- **`BaseEntity`** — `Id (long, PK)`.
- **`BaseAuditableEntity : BaseEntity`** — thêm `CreatedAt (DateTime)`, `UpdatedAt (DateTime?)`.

Cột `Id` được bỏ qua ở các bảng bên dưới (mọi bảng đều có). Cột audit chỉ ghi chú "✅ Audit" ở tiêu đề bảng nếu có.

---

## 1. Catalog — `Domain/Entities/Catalog`

Cây danh mục → sản phẩm (model cha) → biến thể (Product con) + thuộc tính (EAV) → ảnh/video. Xem ADR 0003.

### `Categories` ✅ Audit — danh mục dạng cây (iPhone, iPad, Mac…)

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `ParentId` | `long?` | FK → `Categories.Id`, `OnDelete: Restrict` | Danh mục cha; `null` = danh mục gốc. Tự tham chiếu tạo cây. |
| `Name` | `string` | `varchar(200)`, NOT NULL | Tên danh mục hiển thị. |
| `Slug` | `string` | `varchar(200)`, NOT NULL, **UNIQUE** | Định danh URL SEO (vd `/iphone`). |
| `SortOrder` | `int` | | Thứ tự sắp xếp trong menu/listing. |
| `SeoContent` | `string?` | `text` | Đoạn nội dung SEO ở chân trang danh mục (tùy chọn). |

Quan hệ điều hướng: `Parent`, `Children`, `Products`.

### `Products` ✅ Audit — sản phẩm. Tự tham chiếu cha–con (ADR 0003)

> **Mô hình (ADR [0003](decisions/0003-product-attribute-eav.md)):** `ProductParentId == null` = **model cha** (gom + hiển thị,
> **không bán trực tiếp**, giá 0, không SKU). `ProductParentId` có giá trị = **biến thể con** (đơn vị **bán**: có giá/SKU/tồn kho).
> Thuộc tính phân biệt biến thể (dung lượng, màu, RAM…) lưu ở [`ProductAttributes`](#productattributes--giá-trị-thuộc-tính-eav). **Không còn bảng `ProductVariants`.**

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `CategoryId` | `long` | FK → `Categories.Id`, `OnDelete: Restrict` | Danh mục. |
| `ProductParentId` | `long?` | FK → `Products.Id` (self), `OnDelete: Restrict` | `null` = model cha; có giá trị = biến thể con của model này. |
| `Name` | `string` | `varchar(300)`, NOT NULL | Tên sản phẩm (con dùng chung tên model cha). |
| `Slug` | `string` | `varchar(300)`, NOT NULL, **UNIQUE** | Slug URL SEO (cha + mỗi con đều có slug riêng). |
| `Sku` | `string?` | `varchar(80)`, **UNIQUE (lọc NOT NULL)** | Mã SKU — chỉ đặt cho biến thể con. |
| `BasePrice` | `decimal(18,2)` | | Giá bán niêm yết của biến thể con (cha = 0). Giá hiệu lực qua `IPricingService`. |
| `CompareAtPrice` | `decimal(18,2)?` | | Giá gạch ngang; `null` = không hiển thị. |
| `Status` | `ProductStatus` (int) | mặc định `Active` | `Active`/`OutOfStock`/`Discontinued` (trước là `VariantStatus`). |
| `Tagline` | `string?` | `varchar(200)` | Khẩu hiệu ngắn cho ProductCard. |
| `Description` | `string?` | `text` | Mô tả dài. |
| `Brand` | `string?` | `varchar(100)` | Thương hiệu. |
| `SpecsJson` | `string?` | **`jsonb`** | Thông số kỹ thuật JSON. |

Quan hệ: `Category`, `Parent`/`Children` (self), `Attributes`, `Images`, `Videos`.

### `Attributes` — định nghĩa loại thuộc tính (master, EAV) *(không audit)*

> CLR class `Attribute` (alias `AttributeEntity` để tránh trùng `System.Attribute`).

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `Name` | `string` | `varchar(100)`, NOT NULL, **UNIQUE** | Tên loại thuộc tính (vd "Dung lượng", "Màu", "RAM"). |
| `SortOrder` | `int` | | Thứ tự hiển thị (ghép nhãn biến thể theo thứ tự này). |

### `ProductAttributes` — giá trị thuộc tính (EAV) *(không audit)*

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `AttributeId` | `long` | FK → `Attributes.Id`, `OnDelete: Restrict` | Loại thuộc tính. |
| `ProductId` | `long` | FK → `Products.Id`, `OnDelete: Cascade` | Sản phẩm/biến thể mang giá trị. |
| `Value` | `string` | `varchar(500)`, NOT NULL | Giá trị (vd "256GB", "Đen"). |
| `CreatedDate` | `DateTime` | NOT NULL | Thời điểm tạo (giờ server). |

UNIQUE `(ProductId, AttributeId)` — mỗi sản phẩm 1 giá trị/loại thuộc tính.

### `ProductImages` — ảnh sản phẩm *(không audit)*

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `ProductId` | `long` | FK → `Products.Id` (cascade theo convention) | Sản phẩm chứa ảnh (gallery gắn ở model cha). |
| `Url` | `string` | `varchar(500)`, NOT NULL | Đường dẫn ảnh. |
| `SortOrder` | `int` | | Thứ tự hiển thị trong gallery. |

### `ProductVideos` — video sản phẩm *(không audit)*

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `ProductId` | `long` | FK → `Products.Id` | Sản phẩm chứa video. |
| `YoutubeUrl` | `string` | `text`, NOT NULL | URL video YouTube. |

---

## 2. Pricing — `Domain/Entities/Pricing`

Khuyến mãi nhiều tầng (SPEC §4.2). **Giá không phân theo vùng** — giá bán nằm trực tiếp trên
`Product.BasePrice` (+ `CompareAtPrice`) của biến thể con, không còn bảng `Region`/`PriceByRegion`.
Điều kiện KM (`ConditionsJson`): `variantIds` khớp Id biến thể con, `productIds` khớp Id model cha, `categoryIds` khớp danh mục.

### `Promotions` ✅ Audit — khuyến mãi nhiều tầng

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `Name` | `string` | `varchar(200)`, NOT NULL | Tên chương trình. |
| `Type` | `PromotionType` (int) | | Loại: `Percentage`/`FixedAmount`/`Gift`/`Voucher`/`Combo`/`BankOffer`. |
| `Value` | `decimal(18,2)` | | Giá trị giảm (theo % hoặc số tiền tùy `Type`). |
| `StartsAt` | `DateTime` | index `(StartsAt, EndsAt)` | Bắt đầu hiệu lực. |
| `EndsAt` | `DateTime` | index `(StartsAt, EndsAt)` | Kết thúc hiệu lực. |
| `ConditionsJson` | `string?` | **`jsonb`** | Điều kiện nhắm mục tiêu (P2-05). `null`/rỗng = áp **toàn bộ**. |

Phương thức domain: `IsActiveAt(at)` → `at` nằm trong `[StartsAt, EndsAt]`.

**`ConditionsJson` (P2-05)** — KM chỉ áp khi variant khớp ≥1 danh sách được khai (danh sách bỏ trống = không ràng buộc theo trục đó):
```json
{ "categoryIds": [1], "productIds": [10], "variantIds": [100] }
```
Pricing (`IPricingService`): áp **1 `Percentage` tốt nhất → rồi 1 `FixedAmount` tốt nhất** trên giá đã giảm (clamp ≥ 0).
`Gift`/`Voucher`/`Combo`/`BankOffer` **không đổi giá** (chỉ liệt kê ở khối Ưu đãi). `minQuantity`/`minSubtotal` chưa hỗ trợ (cần ngữ cảnh giỏ).

### `PaymentPromotions` ✅ Audit — ưu đãi thanh toán theo ngân hàng/ví

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `Bank` | `string` | `varchar(100)`, NOT NULL | Ngân hàng/ví (VNPAY, MB…). |
| `Title` | `string` | `varchar(300)`, NOT NULL | Tiêu đề ưu đãi. |
| `Description` | `string?` | `text` | Mô tả chi tiết. |
| `StartsAt` | `DateTime` | | Bắt đầu hiệu lực. |
| `EndsAt` | `DateTime` | | Kết thúc hiệu lực. |

---

## 3. Inventory — `Domain/Entities/Inventory`

### `Stores` ✅ Audit — cửa hàng trong chuỗi (định vị + tồn kho)

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `Name` | `string` | `text`, NOT NULL | Tên cửa hàng. |
| `Address` | `string?` | `text` | Địa chỉ. |
| `Lat` | `double?` | | Vĩ độ (định vị bản đồ). |
| `Lng` | `double?` | | Kinh độ. |
| `Phone` | `string?` | `text` | Số điện thoại. |

### `Inventories` ✅ Audit — tồn kho 1 biến thể tại 1 cửa hàng

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `ProductId` | `long` | FK → `Products.Id` (cascade theo convention) | Biến thể (Product con) được tính tồn. |
| `StoreId` | `long` | FK → `Stores.Id` | Cửa hàng. |
| `Quantity` | `int` | | Số lượng tồn ("cửa hàng có sẵn"). |

---

## 4. Sales — `Domain/Entities/Sales`

Giỏ hàng → đơn hàng → thanh toán/giao hàng; combo phụ kiện.

### `Carts` ✅ Audit — giỏ hàng

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `CustomerId` | `long?` | | Khách đăng nhập (FK → `AspNetUsers`); `null` nếu khách vãng lai. |
| `SessionId` | `string?` | `text` | Định danh phiên cho khách vãng lai. |

Quan hệ: `Items` (CartItem). Lưu ý: giỏ khách vãng lai có thể cache ở Redis (xem `CacheKeys`).

### `CartItems` — dòng giỏ hàng *(không audit)*

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `CartId` | `long` | FK → `Carts.Id` | Giỏ chứa dòng. |
| `ProductId` | `long` | FK → `Products.Id` | Biến thể (Product con) được thêm. |
| `Quantity` | `int` | | Số lượng. |
| `UnitPrice` | `decimal(18,2)` | | Đơn giá snapshot lúc thêm. **Hiển thị giỏ dùng giá hiệu lực hiện tại qua `IPricingService`** (phản ánh KM đang chạy), không tin cột này — tránh giá cũ. Chốt cứng khi đặt hàng (`OrderItem.UnitPrice`). |
| `UnitPriceOverride` | `decimal(18,2)?` | | Giá cố định cho dòng (P3-01 mua kèm = `BundleItem.BundlePrice`). `null` = dùng giá hiệu lực `IPricingService`; có giá trị = giỏ/checkout **dùng đúng giá này** (không định giá lại). |

### `Orders` ✅ Audit — đơn hàng

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `CustomerId` | `long?` | | Khách đặt hàng (user đăng nhập); **`null` = khách vãng lai** (guest checkout, P2-02). Không đặt FK cứng. |
| `Status` | `OrderStatus` (int) | mặc định `Pending` | `Pending`/`Confirmed`/`Processing`/`Shipped`/`Delivered`/`Cancelled`/`Refunded`. |
| `Total` | `decimal(18,2)` | | Tổng tiền đơn. |
| `PaymentMethod` | `string?` | `text` | Phương thức thanh toán đã chọn. |

Quan hệ: `Items` (OrderItem), `Payments`, `Shipment` (0..1).

### `OrderItems` — dòng đơn hàng *(không audit)*

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `OrderId` | `long` | FK → `Orders.Id` | Đơn chứa dòng. |
| `ProductId` | `long` | FK → `Products.Id`, `OnDelete: Restrict` | Biến thể (Product con) đã mua. Restrict: không cascade xoá lịch sử đơn. |
| `Quantity` | `int` | | Số lượng. |
| `UnitPrice` | `decimal(18,2)` | | Đơn giá chốt tại thời điểm đặt (snapshot). |

### `Payments` ✅ Audit — giao dịch thanh toán

> Ủy thác cổng PCI-DSS — **KHÔNG** lưu dữ liệu thẻ (SPEC §13).

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `OrderId` | `long` | FK → `Orders.Id` | Đơn được thanh toán. |
| `Provider` | `string?` | `text` | Cổng thanh toán (VNPAY/ZaloPay…). |
| `Status` | `PaymentStatus` (int) | mặc định `Pending` | `Pending`/`Authorized`/`Paid`/`Failed`/`Refunded`. |
| `Amount` | `decimal(18,2)` | | Số tiền giao dịch. |
| `TxnRef` | `string?` | `text` | Mã tham chiếu giao dịch từ cổng. |

### `Shipments` ✅ Audit — vận đơn

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `OrderId` | `long` | FK → `Orders.Id` | Đơn được giao. |
| `Address` | `string?` | `text` | Địa chỉ giao (snapshot). |
| `Status` | `ShipmentStatus` (int) | mặc định `Pending` | `Pending`/`Preparing`/`Shipped`/`Delivered`/`Returned`. |
| `TrackingNo` | `string?` | `text` | Mã vận đơn. |

### `Bundles` ✅ Audit — combo "mua kèm phụ kiện" cho 1 sản phẩm chính

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `MainProductId` | `long` | FK → `Products.Id` | Sản phẩm chính của combo. |

Quan hệ: `Items` (BundleItem).

### `BundleItems` — phụ kiện trong combo *(không audit)*

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `BundleId` | `long` | FK → `Bundles.Id` | Combo cha. |
| `AccessoryProductId` | `long` | FK → `Products.Id`, `OnDelete: Restrict` | Biến thể phụ kiện (Product con) mua kèm. |
| `BundlePrice` | `decimal(18,2)` | | Giá ưu đãi khi mua kèm. |

---

## 5. Customers — `Domain/Entities/Customers`

### `Addresses` ✅ Audit — sổ địa chỉ nhận hàng

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `CustomerId` | `long` | FK → `AspNetUsers.Id` | Khách sở hữu địa chỉ. |
| `Recipient` | `string` | `text`, NOT NULL | Tên người nhận. |
| `Phone` | `string` | `text`, NOT NULL | SĐT người nhận. |
| `AddressLine` | `string` | `text`, NOT NULL | Địa chỉ chi tiết. |
| `IsDefault` | `bool` | | Địa chỉ mặc định. |

---

## 6. Reviews — `Domain/Entities/Reviews`

### `Reviews` ✅ Audit — đánh giá sản phẩm

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `ProductId` | `long` | FK → `Products.Id`, `OnDelete: Restrict` | Biến thể (Product con) được đánh giá. |
| `CustomerId` | `long?` | | Người đánh giá; `null` nếu ẩn danh/khách vãng lai. |
| `Rating` | `int` | 1..5 | Số sao. |
| `Content` | `string?` | `text` | Nội dung nhận xét. |

---

## 7. CMS — `Domain/Entities/Cms`

### `Articles` ✅ Audit — tin tức/blog

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `Title` | `string` | `varchar(300)`, NOT NULL | Tiêu đề. |
| `Slug` | `string` | `varchar(300)`, NOT NULL, **UNIQUE** | Slug URL. |
| `Body` | `string?` | `text` | Nội dung. |
| `PublishedAt` | `DateTime?` | | Thời điểm xuất bản; `null` = nháp. |

### `Pages` ✅ Audit — trang nội dung tĩnh (chính sách, đổi trả…)

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `Slug` | `string` | `varchar(200)`, NOT NULL, **UNIQUE** | Slug URL. |
| `Title` | `string` | `varchar(300)`, NOT NULL | Tiêu đề. |
| `Body` | `string?` | `text` | Nội dung. |

### `Banners` ✅ Audit — slide carousel trang chủ

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `Eyebrow` | `string?` | `varchar(80)` | Nhãn nhỏ phía trên tiêu đề (vd "Vừa ra mắt"). |
| `Title` | `string` | `varchar(200)`, NOT NULL | Tiêu đề chính. |
| `Subtitle` | `string?` | `varchar(300)` | Phụ đề. |
| `ImageUrl` | `string?` | `varchar(500)` | Ảnh nền slide. |
| `LinkUrl` | `string?` | `varchar(500)` | Đích khi bấm CTA (slug nội bộ, vd `/iphone`). |
| `CtaText` | `string?` | `varchar(60)` | Nhãn nút CTA. |
| `SortOrder` | `int` | index `(IsActive, SortOrder)` | Thứ tự slide. |
| `IsActive` | `bool` | mặc định `true`, index `(IsActive, SortOrder)` | Bật/tắt hiển thị. |
| `StartsAt` | `DateTime?` | | Mốc bắt đầu hiển thị; `null` = không giới hạn đầu. |
| `EndsAt` | `DateTime?` | | Mốc kết thúc; `null` = không giới hạn cuối. |

Phương thức domain: `IsActiveAt(at)` → `IsActive` **và** `at` nằm trong cửa sổ `[StartsAt, EndsAt]` (bỏ qua mốc `null`). Admin CRUD ở Phase 5.

---

## 8. Services — `Domain/Entities/Services`

### `ServicePackages` ✅ Audit — gói bảo hành/AppleCare

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `Type` | `string` | `text`, NOT NULL | Loại gói (`warranty`/`applecare`/`extended`…). |
| `Name` | `string` | `text`, NOT NULL | Tên gói. |
| `Description` | `string?` | `text` | Mô tả quyền lợi. |
| `DurationMonths` | `int` | | Thời hạn (tháng). |
| `Price` | `decimal(18,2)` | | Giá gói. |

### `TradeInRequests` ✅ Audit — yêu cầu thu cũ đổi mới

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `CustomerId` | `long?` | | Khách gửi yêu cầu; `null` nếu chưa đăng nhập. |
| `Model` | `string` | `text`, NOT NULL | Model máy cũ. |
| `Condition` | `string?` | `text` | Tình trạng máy. |
| `EstimatedPrice` | `decimal(18,2)?` | | Giá thu ước tính. |
| `Status` | `TradeInStatus` (int) | mặc định `Pending` | `Pending`/`Quoted`/`Accepted`/`Rejected`/`Completed`. |

### `StockNotifications` ✅ Audit — "báo khi có hàng"

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `ProductId` | `long` | FK → `Products.Id` | Biến thể (Product con) hết hàng được theo dõi. |
| `Contact` | `string` | `text`, NOT NULL | Email hoặc SĐT nhận thông báo. |
| `Notified` | `bool` | | Đã gửi thông báo chưa. |

---

## 9. Localization — `Domain/Entities/Localization`

### `Translations` — bản dịch nội dung động *(không audit)*

| Cột | Kiểu | Ràng buộc | Ý nghĩa |
|---|---|---|---|
| `Entity` | `string` | `varchar(80)`, NOT NULL | Tên entity được dịch ("Product", "Category"…). |
| `EntityId` | `long` | | Id bản ghi gốc. |
| `Lang` | `string` | `varchar(8)`, NOT NULL | Ngôn ngữ (`vi`/`en`). |
| `Field` | `string` | `varchar(80)`, NOT NULL | Tên trường được dịch ("Name"/"Description"). |
| `Value` | `string` | `text`, NOT NULL | Nội dung đã dịch. |

Ràng buộc duy nhất tổ hợp **`(Entity, EntityId, Lang, Field)`** — mỗi trường/ngôn ngữ chỉ 1 bản dịch.
Chuỗi UI tĩnh dùng `.resx`; bảng này chỉ cho **nội dung DB** (VI/EN).

---

## 10. Identity (khách hàng & phân quyền) — `Infrastructure/Identity`

Dùng ASP.NET Core Identity với khóa `long`. `Customer` của SPEC được gộp vào `ApplicationUser`.

### `AspNetUsers` — `ApplicationUser : IdentityUser<long>`

| Cột | Kiểu | Ý nghĩa |
|---|---|---|
| `FullName` | `string?` | Họ tên đầy đủ (field mở rộng của HtMobile). |
| `UserName`, `NormalizedUserName` | `string?` | Tên đăng nhập + bản chuẩn hóa (index). |
| `Email`, `NormalizedEmail`, `EmailConfirmed` | `string?` / `bool` | Email + xác thực. |
| `PhoneNumber`, `PhoneNumberConfirmed` | `string?` / `bool` | SĐT + xác thực. |
| `PasswordHash`, `SecurityStamp`, `ConcurrencyStamp` | `string?` | Hash mật khẩu + dấu bảo mật/đồng thời. |
| `TwoFactorEnabled`, `LockoutEnd`, `LockoutEnabled`, `AccessFailedCount` | | 2FA & khóa tài khoản. |

Các bảng Identity còn lại (chuẩn): `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`,
`AspNetRoleClaims`, `AspNetUserLogins`, `AspNetUserTokens`. Vai trò khai báo ở `Roles`: **`Admin`**, **`Customer`**.

---

## 11. Enums — `Domain/Enums/Enums.cs`

Lưu DB dưới dạng `int` (giá trị trong ngoặc).

| Enum | Giá trị |
|---|---|
| `OrderStatus` | `Pending(0)`, `Confirmed(1)`, `Processing(2)`, `Shipped(3)`, `Delivered(4)`, `Cancelled(5)`, `Refunded(6)` |
| `PaymentStatus` | `Pending(0)`, `Authorized(1)`, `Paid(2)`, `Failed(3)`, `Refunded(4)` |
| `ShipmentStatus` | `Pending(0)`, `Preparing(1)`, `Shipped(2)`, `Delivered(3)`, `Returned(4)` |
| `PromotionType` | `Percentage(0)`, `FixedAmount(1)`, `Gift(2)`, `Voucher(3)`, `Combo(4)`, `BankOffer(5)` |
| `ProductStatus` | `Active(0)`, `OutOfStock(1)`, `Discontinued(2)` (trước là `VariantStatus`) |
| `TradeInStatus` | `Pending(0)`, `Quoted(1)`, `Accepted(2)`, `Rejected(3)`, `Completed(4)` |

---

## 12. Hành vi xóa FK (tóm tắt phần cấu hình tường minh)

| Quan hệ | OnDelete | Lý do |
|---|---|---|
| `Category.Parent` → `Category` | **Restrict** | Tránh xóa dây chuyền cả nhánh cây. |
| `Product.Category` | **Restrict** | Không cho xóa danh mục còn sản phẩm. |
| `Product.Parent` → `Product` (self) | **Restrict** | Xóa model cha không tự xoá biến thể con (xử lý ở nghiệp vụ). |
| `ProductAttribute.Product` | **Cascade** | Xóa sản phẩm/biến thể → xoá giá trị thuộc tính của nó. |
| `ProductAttribute.Attribute` | **Restrict** | Không cho xoá loại thuộc tính còn được dùng. |
| `OrderItem.Product` / `Review.Product` / `CartItem.Product` / `BundleItem.AccessoryProduct` | **Restrict** | Không cascade xoá lịch sử đơn/đánh giá/giỏ/bundle khi xoá sản phẩm (ADR 0003). |

Các FK khác dùng quy ước mặc định của EF Core (`Cascade` cho FK bắt buộc, `SetNull`/`Restrict` cho FK nullable tùy ngữ cảnh).

---

## 13. Chỉ mục & extension đáng chú ý

- **UNIQUE slug**: `Categories.Slug`, `Products.Slug` (cha + con), `Articles.Slug`, `Pages.Slug`.
- **UNIQUE khác**: `Products.Sku` (lọc `IS NOT NULL`), `Attributes.Name`, `ProductAttributes(ProductId, AttributeId)`, `Translations(Entity, EntityId, Lang, Field)`.
- **Index lọc**: `Promotions(StartsAt, EndsAt)`, `Banners(IsActive, SortOrder)`.
- **Extension Postgres**: `pg_trgm` (phục vụ full-text/trigram autocomplete — SPEC §6).

> Cache Redis (không phải bảng) chuẩn hóa ở `Domain/Constants/CacheKeys.cs`:
> `catalog:category-tree`, `pricing:variant:{id}`, `pricing:product:{id}:promotions`, `search:suggest:{query}`.
