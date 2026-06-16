# Apple Reseller E-commerce — Technical Spec (clone kiểu ShopDunk)

> Tài liệu đặc tả kỹ thuật để xây dựng website thương mại điện tử chuỗi bán lẻ sản phẩm Apple
> (iPhone, iPad, Mac, Watch, Phụ kiện, Âm thanh, Camera, Gia dụng, máy cũ) kèm hệ sinh thái dịch vụ.

---

## 1. Tổng quan

Website bán lẻ chuỗi cửa hàng Apple với các đặc thù nghiệp vụ:
- Giá bán **theo khu vực địa lý** (miền Bắc / miền Nam).
- Hệ thống **khuyến mãi nhiều tầng** (giảm %, quà tặng, voucher, combo, ưu đãi theo ngân hàng).
- **Trả góp 0%** (qua công ty tài chính & qua thẻ tín dụng).
- **Thu cũ đổi mới** (định giá máy cũ, trợ giá lên đời).
- Nhiều **gói dịch vụ bảo hành** (tiêu chuẩn, kim cương, VIP 1 đổi 1, mở rộng, AppleCare).
- **Đa ngôn ngữ** (VI/EN).
- SEO-heavy: trang danh mục có nội dung mô tả dài.

Tham chiếu: bản gốc chạy trên nền **nopCommerce (ASP.NET Core)**, media phục vụ qua CDN riêng sau Cloudflare. Stack có thể thay thế (xem mục 11).

---

## 2. Kiến trúc tổng thể

```
[Browser] ──► [Storefront (SSR)] ──► [Backend API] ──► [PostgreSQL]
     │                                     │
     ├──► [CDN: ảnh/video/asset]           ├──► [Redis cache/session]
     │                                     ├──► [Search engine]
     └──► [Live chat / Hotline widget]     └──► [3rd-party: Payment, Trả góp, Vận chuyển]

[Admin / CMS] ──► [Backend API]
```

Nguyên tắc:
- Tách **storefront** (khách) và **admin/CMS** (quản trị).
- Tách **pricing engine** (giá + khuyến mãi) thành service/module riêng.
- Media (ảnh nặng, video) phục vụ qua **CDN riêng**, không đi qua app server.
- Thanh toán ủy thác hoàn toàn cho cổng đạt chuẩn PCI-DSS (không tự lưu dữ liệu thẻ).

---

## 3. Sitemap

| Loại trang | Đường dẫn ví dụ | Ghi chú |
|---|---|---|
| Trang chủ | `/` | Banner slider, khối sản phẩm theo danh mục, newsfeed, footer |
| Danh mục | `/iphone`, `/ipad`, `/mac`, `/apple-watch`, `/phu-kien`, `/am-thanh`, `/camera`, `/gia-dung`, `/may-cu` | Breadcrumb, banner, "Sản phẩm HOT", tab lọc series, lưới SP, block SEO |
| Chi tiết SP | `/dien-thoai-iphone-17-pro-max-256gb` | Slug riêng cho mỗi biến thể; xem mục 5 |
| Giỏ hàng | `/cart` | Có empty state |
| Đăng nhập | `/login?ReturnUrl=...` | |
| Tài khoản | `/customer/info` | Yêu cầu đăng nhập |
| Đơn hàng | `/customer/orders` | |
| Dịch vụ | `/dich-vu` + trang con | AppleCare, bảo hành, Office, Business/School Manager... |
| Tin tức/Blog | `/newsfeed`, `/{article-slug}` | |
| Trang chính sách | Giao hàng, đổi trả, bảo hành, bảo mật, check IMEI, tra cứu hóa đơn, hệ thống cửa hàng, tuyển dụng | Tĩnh/CMS |
| Đổi ngôn ngữ | `/changelanguage/{langId}?returnUrl=...` | |

---

## 4. Danh mục tính năng (Feature List)

### 4.1 Storefront (khách hàng)
- [ ] Header cố định: logo, ô tìm kiếm, giỏ hàng, tài khoản, chọn ngôn ngữ.
- [ ] Thanh điều hướng danh mục + **mega menu** ("Dịch vụ" dropdown nhiều mục).
- [ ] **Tìm kiếm autocomplete**: gợi ý từ khóa + gợi ý sản phẩm (thumbnail, tên, giá gốc/giá KM).
- [ ] Trang danh mục: tab lọc theo dòng máy (series), lưới sản phẩm, block SEO.
- [ ] Thẻ sản phẩm: ảnh, badge giảm %, badge "Mới"/"Cũ", giá gạch ngang + giá bán.
- [ ] Trang chi tiết sản phẩm đầy đủ (mục 5).
- [ ] **Giá theo khu vực** (dropdown chọn vùng).
- [ ] Giỏ hàng + cập nhật số lượng + empty state.
- [ ] Checkout: thông tin nhận hàng, phương thức giao, phương thức thanh toán.
- [ ] Đăng nhập / Đăng ký / Quên mật khẩu / Nhớ đăng nhập.
- [ ] Quản lý tài khoản & lịch sử đơn hàng.
- [ ] Đa ngôn ngữ VI/EN.
- [ ] "Theo dõi để biết khi có hàng" (sản phẩm hết hàng).
- [ ] Live chat nổi + nút hotline nổi + scroll-to-top.
- [ ] Khối tin tức/blog.

### 4.2 Nghiệp vụ bán hàng
- [ ] Khuyến mãi nhiều tầng (%, quà, voucher, combo).
- [ ] Ưu đãi thanh toán theo ngân hàng/ví (carousel).
- [ ] Trả góp 0% qua công ty tài chính + qua thẻ.
- [ ] Thu cũ đổi mới (định giá + trợ giá).
- [ ] Bundle "mua kèm phụ kiện" (giá ưu đãi, tổng tiết kiệm).
- [ ] Tồn kho theo cửa hàng ("cửa hàng có sẵn").
- [ ] Gói dịch vụ bảo hành.
- [ ] Bán hàng doanh nghiệp / trường học (báo giá số lượng lớn).
- [ ] Check IMEI, tra cứu hóa đơn điện tử, định vị cửa hàng.

### 4.3 Admin / CMS
- [ ] Quản lý danh mục / sản phẩm / biến thể / tồn kho (theo cửa hàng & vùng).
- [ ] Quản lý khuyến mãi & campaign theo thời gian.
- [ ] Quản lý đơn hàng & trạng thái.
- [ ] CMS: banner, trang tĩnh, blog.
- [ ] Quản lý người dùng & phân quyền.
- [ ] Cấu hình đa ngôn ngữ.
- [ ] Báo cáo doanh thu / đơn hàng.

---

## 5. Trang chi tiết sản phẩm (đặc tả chi tiết)

**Cột media (trái):**
- Video YouTube nhúng giới thiệu.
- Gallery thumbnail cuộn ngang (carousel, prev/next).

**Cột thông tin (phải):**
- Tên sản phẩm.
- Dropdown **chọn khu vực giá** (miền Bắc / miền Nam).
- Giá hiện tại + giá gốc gạch ngang + % giảm.
- Bộ chọn **dung lượng** (nút: 256GB / 512GB / 1TB / 2TB).
- Bộ chọn **màu** (swatch tròn).
  - → Mỗi tổ hợp dung lượng × màu = một **variant/SKU** (URL riêng).
- Khối **Ưu đãi**: danh sách KM có hiệu lực + mốc thời gian + "Xem thêm".
- Khối **Ưu đãi thanh toán**: carousel ưu đãi theo ngân hàng.
- Hành động mua:
  - `MUA NGAY` (primary)
  - `Mua trả góp 0%` (công ty tài chính)
  - `Trả góp 0% qua thẻ` (Visa/Mastercard/JCB/Amex)
  - `Thu cũ đổi mới`
  - Hotline đặt mua + "Cửa hàng có sẵn sản phẩm".
- Cam kết: bộ sản phẩm gồm gì, đổi 1-1 trong 30 ngày, bảo hành 1 năm, giao hàng toàn quốc, hoàn thuế cho người nước ngoài.

**Phía dưới:**
- Bundle "mua kèm phụ kiện": SP chính + phụ kiện gợi ý (giá niêm yết & giá mua kèm), tổng tiết kiệm, nút "Mua N sản phẩm".
- Tab "Sản phẩm tương tự" / "Sản phẩm cũ" (carousel).
- Mô tả rich-media dài.
- Bảng so sánh thông số kỹ thuật giữa nhiều model.
- Disclaimer pháp lý.
- Đánh giá: rating sao, tên, ngày, nội dung, "Xem thêm".

---

## 6. Use cases

| ID | Use case | Mô tả luồng |
|---|---|---|
| UC-01 | Tìm & mua nhanh | Gõ từ khóa → autocomplete → vào PDP → chọn dung lượng/màu → xem giá theo vùng → Mua ngay / thêm giỏ |
| UC-02 | Mua kèm phụ kiện | Tại PDP chọn phụ kiện gợi ý → mua combo 1 lần |
| UC-03 | Trả góp | Chọn "Trả góp 0%" → nhập thông tin → duyệt hồ sơ qua công ty tài chính / thanh toán thẻ |
| UC-04 | Thu cũ đổi mới | Khai model + tình trạng máy cũ → ước tính giá thu → áp trợ giá vào đơn mới |
| UC-05 | Theo dõi hàng về | SP hết hàng → đăng ký email/SĐT nhận thông báo |
| UC-06 | Khách doanh nghiệp/trường học | Yêu cầu báo giá số lượng lớn / đăng ký Business/School Manager |
| UC-07 | Sau bán | Mua gói bảo hành mở rộng/kim cương, check IMEI, tra cứu hóa đơn, tìm cửa hàng |
| UC-08 | Khách đã đăng nhập | Xem lịch sử đơn hàng, quản lý thông tin cá nhân |

---

## 7. Mô hình dữ liệu (Data Model)

```
Category(id, parent_id, name, slug, sort_order, seo_content)
Product(id, category_id, name, slug, description, brand, specs_json)
ProductVariant(id, product_id, sku, storage, color, slug, base_price, status)
ProductImage(id, product_id, variant_id?, url, sort_order)
ProductVideo(id, product_id, youtube_url)
Region(id, name)                                  // Bắc / Nam
PriceByRegion(id, variant_id, region_id, price, compare_at_price)
Store(id, name, address, region_id, lat, lng, phone)
Inventory(id, variant_id, store_id, quantity)
Promotion(id, name, type, value, starts_at, ends_at, conditions_json)
PaymentPromotion(id, bank, title, description, starts_at, ends_at)
Bundle(id, main_product_id)
BundleItem(id, bundle_id, accessory_variant_id, bundle_price)
Cart(id, customer_id?, session_id, created_at)
CartItem(id, cart_id, variant_id, quantity, unit_price)
Order(id, customer_id, status, region_id, total, payment_method, created_at)
OrderItem(id, order_id, variant_id, quantity, unit_price)
Payment(id, order_id, provider, status, amount, txn_ref)
Shipment(id, order_id, address, status, tracking_no)
Customer(id, username, email, phone, password_hash)
Address(id, customer_id, recipient, phone, address, is_default)
Review(id, variant_id, customer_id, rating, content, created_at)
Article(id, title, slug, body, published_at)
ServicePackage(id, type, name, description, duration_months, price)
TradeInRequest(id, customer_id?, model, condition, estimated_price, status)
StockNotification(id, variant_id, email/phone, created_at)
Translation(id, entity, entity_id, lang, field, value)
```

---

## 8. Phác thảo API (REST)

```
# Catalog
GET  /api/categories
GET  /api/categories/{slug}/products?series=&page=&sort=
GET  /api/products/{slug}                 # PDP + variants + media + reviews
GET  /api/variants/{slug}?region={id}     # giá theo vùng
GET  /api/search/suggest?q=               # autocomplete (keyword + products)

# Pricing
GET  /api/variants/{id}/price?region={id}
GET  /api/products/{id}/promotions
GET  /api/products/{id}/bundle

# Cart & Order
POST /api/cart/items
PATCH /api/cart/items/{id}
DELETE /api/cart/items/{id}
GET  /api/cart
POST /api/orders
GET  /api/orders/{id}

# Auth & Account
POST /api/auth/register
POST /api/auth/login
POST /api/auth/forgot-password
GET  /api/customer/info
GET  /api/customer/orders

# Services / Nghiệp vụ
POST /api/trade-in/estimate
POST /api/stock-notifications
GET  /api/stores?region=&q=
POST /api/business/quote-request

# CMS
GET  /api/articles?page=
GET  /api/articles/{slug}
GET  /api/pages/{slug}
```

---

## 9. Component UI tái sử dụng

- `Header` (logo, SearchBox + autocomplete panel, CartIcon, AccountMenu, LangSwitcher)
- `MegaMenu` / `CategoryNav`
- `ProductCard` (image, discount badge, new/used badge, price)
- `Carousel` (banner / related / payment-offers)
- `VariantSelector` (storage buttons + color swatches)
- `RegionSelector`
- `PriceBlock` (current + compare-at + % off)
- `OfferList` / `PaymentOfferCarousel`
- `BundleWidget`
- `SpecComparisonTable`
- `ReviewList` (star rating)
- `Tabs` / `Accordion`
- `FloatingButtons` (live chat, hotline, scroll-to-top)
- `Footer` (multi-column)
- `EmptyState` (cart trống)

> Responsive, **mobile-first**.

---

## 10. SEO & Performance

- SSR/SSG cho trang chủ, danh mục, PDP, blog.
- Structured data: `schema.org/Product`, `BreadcrumbList`, `Review`.
- Ảnh responsive nhiều size (thumbnail dùng hậu tố `_240`), lazy-load.
- Phục vụ media qua CDN; tách object storage (S3-compatible).
- Cache giá/khuyến mãi & catalog ở Redis; invalidate khi cập nhật.

---

## 11. Gợi ý Tech Stack

**Phương án A — nhanh, có sẵn nghiệp vụ:** nopCommerce (ASP.NET Core, C#, SQL Server).

**Phương án B — hiện đại, linh hoạt:**
- Frontend: **Next.js (React) + Tailwind CSS** (SSR cho SEO).
- Backend: **NestJS (Node.js)** hoặc headless **Medusa.js** / Laravel (PHP).
- DB: **PostgreSQL** (giao dịch) + **Redis** (cache/session).
- Search: **Meilisearch / Typesense / Elasticsearch** (autocomplete).
- Media: **CDN (Cloudflare)** + object storage (S3) + image optimizer.
- Tích hợp: **VNPAY / ZaloPay** (thanh toán), API trả góp & thu cũ, widget Zalo/Facebook (live chat).

---

## 12. Lộ trình implement (gợi ý theo phase)

**Phase 1 — Nền tảng & catalog**
- Setup repo, CI/CD, DB schema, seed danh mục/sản phẩm.
- Trang chủ, danh mục, PDP cơ bản, tìm kiếm + autocomplete.

**Phase 2 — Mua hàng**
- Giỏ hàng, checkout, đăng ký/đăng nhập, tài khoản & đơn hàng.
- Pricing engine: giá theo vùng + khuyến mãi cơ bản.

**Phase 3 — Nghiệp vụ đặc thù**
- Bundle mua kèm, trả góp, thu cũ đổi mới, theo dõi hàng về.
- Tích hợp cổng thanh toán.

**Phase 4 — Dịch vụ & CMS**
- Gói bảo hành, bán hàng doanh nghiệp, blog/newsfeed, trang chính sách.
- Đa ngôn ngữ, định vị cửa hàng, check IMEI.

**Phase 5 — Hoàn thiện**
- Admin/CMS đầy đủ, báo cáo, SEO nâng cao, tối ưu hiệu năng, kiểm thử bảo mật.

---

## 13. Bảo mật & tuân thủ

- HTTPS bắt buộc; mã hóa mật khẩu (bcrypt/argon2).
- Không lưu dữ liệu thẻ — ủy thác cổng thanh toán PCI-DSS.
- Rate-limit cho login/search/checkout.
- CSRF/XSS protection; validate & sanitize input.
- Tuân thủ quy định bảo vệ dữ liệu cá nhân (privacy policy, consent).