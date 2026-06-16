# Data model (tham chiếu — nguồn gốc SPEC §7)

Khóa chính `long`. JSON → `jsonb`. Tên bảng/cột **PascalCase** (mặc định EF Core). Khách hàng = `ApplicationUser : IdentityUser<long>`
(Infrastructure/Identity) — gộp các field của `Customer` trong SPEC.

## Catalog (`Domain/Entities/Catalog`)
- `Category(Id, ParentId?, Name, Slug, SortOrder, SeoContent)` — self-reference cây danh mục.
- `Product(Id, CategoryId, Name, Slug, Tagline?, Description, Brand, SpecsJson)` — `Tagline` = khẩu hiệu ngắn cho ProductCard.
- `ProductVariant(Id, ProductId, Sku, Storage, Color, Slug, BasePrice, Status)` — tổ hợp dung lượng×màu = 1 SKU.
- `ProductImage(Id, ProductId, VariantId?, Url, SortOrder)`
- `ProductVideo(Id, ProductId, YoutubeUrl)`

## Pricing (`Domain/Entities/Pricing`)
- `Region(Id, Name)` — Bắc / Nam.
- `PriceByRegion(Id, VariantId, RegionId, Price, CompareAtPrice)`
- `Promotion(Id, Name, Type, Value, StartsAt, EndsAt, ConditionsJson)` — Type = enum `PromotionType`.
- `PaymentPromotion(Id, Bank, Title, Description, StartsAt, EndsAt)`

## Inventory (`Domain/Entities/Inventory`)
- `Store(Id, Name, Address, RegionId, Lat, Lng, Phone)`
- `Inventory(Id, VariantId, StoreId, Quantity)`

## Sales (`Domain/Entities/Sales`)
- `Cart(Id, CustomerId?, SessionId, CreatedAt)` · `CartItem(Id, CartId, VariantId, Quantity, UnitPrice)`
- `Order(Id, CustomerId, Status, RegionId, Total, PaymentMethod, CreatedAt)` · `OrderItem(...)`
- `Payment(Id, OrderId, Provider, Status, Amount, TxnRef)` · `Shipment(Id, OrderId, Address, Status, TrackingNo)`
- `Bundle(Id, MainProductId)` · `BundleItem(Id, BundleId, AccessoryVariantId, BundlePrice)`

## Customers (`Domain/Entities/Customers`)
- `Address(Id, CustomerId, Recipient, Phone, AddressLine, IsDefault)`

## Khác
- `Review(Id, VariantId, CustomerId, Rating, Content, CreatedAt)` — `Domain/Entities/Reviews`
- `Article(Id, Title, Slug, Body, PublishedAt)`, `Page(Id, Slug, Title, Body)` — `Cms`
- `Banner(Id, Eyebrow?, Title, Subtitle?, ImageUrl?, LinkUrl?, CtaText?, SortOrder, IsActive, StartsAt?, EndsAt?)` — `Cms`; slide carousel trang chủ, hiển thị theo `IsActive` + cửa sổ thời gian (`IsActiveAt`). Admin CRUD ở Phase 5.
- `ServicePackage(Id, Type, Name, Description, DurationMonths, Price)` — gói bảo hành/AppleCare
- `TradeInRequest(Id, CustomerId?, Model, Condition, EstimatedPrice, Status)`
- `StockNotification(Id, VariantId, Contact, CreatedAt)`
- `Translation(Id, Entity, EntityId, Lang, Field, Value)` — đa ngôn ngữ nội dung

## Enums (`Domain/Enums`)
`OrderStatus`, `PaymentStatus`, `ShipmentStatus`, `PromotionType` (Percentage, Gift, Voucher, Combo, BankOffer),
`VariantStatus` (Active, OutOfStock, Discontinued), `TradeInStatus`.
