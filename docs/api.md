# API contract (REST — nguồn gốc SPEC §8)

Controller đặt ở `src/HtMobile.Web/Api`, attribute-routed `[Route("api/...")]`, trả JSON.
Trang khách (SSR) gọi service Application trực tiếp; API phục vụ autocomplete/htmx/đối tác.

## Catalog
```
GET  /api/categories
GET  /api/categories/{slug}/products?series=&page=&sort=
GET  /api/products/{slug}                 # PDP + variants + media + reviews
GET  /api/variants/{slug}                 # chi tiết + giá hiệu lực
GET  /api/search/suggest?q=               # autocomplete (keyword + products)
```

## Storefront (SSR — HTML, không phải JSON)
Route phẳng theo slug + trang SEO. Controller ở `Areas/Storefront`.
```
GET  /                       # Trang chủ (banner, danh mục, SP nổi bật)
GET  /{slug}                 # ISlugResolver → Category | PDP (variant) | 404
GET  /search?q=              # Trang kết quả tìm kiếm (server-render, <meta robots=noindex>)
GET  /search/suggest?q=      # Partial HTML cho htmx autocomplete (gợi ý SP + "xem tất cả")
GET  /sitemap.xml            # Sitemap (danh mục + variant chuẩn mỗi SP)
GET  /robots.txt             # Tĩnh ở wwwroot, trỏ tới /sitemap.xml
```

### Giỏ hàng (MVC + htmx — partial HTML, không phải REST JSON)
Giỏ gắn `CustomerId` (user) hoặc `SessionId` = cookie GUID `htm_cart` (guest). Mọi POST có antiforgery token.
```
GET  /cart                       # Trang giỏ (server-render: list item + tóm tắt | empty state)
POST /cart/items                 # body: variantId, quantity=1 → thêm; redirect về /cart
POST /cart/items/{id}/inc        # +1 → trả partial _CartBody (htmx)
POST /cart/items/{id}/dec        # -1 (về 0 thì xoá dòng) → partial _CartBody (htmx)
POST /cart/items/{id}/remove     # xoá dòng → partial _CartBody (htmx)
POST /bundle/add                 # body: mainVariantId, accessoryVariantIds[] → thêm SP chính + phụ kiện mua kèm (giá BundlePrice từ DB) → /cart
```
> Badge số lượng ở header: ViewComponent `CartBadge`. Đơn giá mỗi dòng = giá hiệu lực qua `IPricingService`.
> Merge giỏ guest→user: `ICartService.MergeAsync` (gọi sau khi đăng nhập — nối ở feature auth P2).
> REST `/api/cart/*` (SPEC §8) để dành cho đối tác/app sau; storefront dùng route MVC trên.

### Tài khoản / Auth (ASP.NET Identity — cookie, email + mật khẩu)
`AccountController` (Storefront). Mọi POST có antiforgery. Đăng nhập/đăng ký thành công → gộp giỏ guest.
```
GET  /register                       # form đăng ký (email, mật khẩu, họ tên)
POST /register                       # tạo user + role Customer + đăng nhập + merge giỏ
GET  /login?ReturnUrl=               # form đăng nhập (email, mật khẩu, nhớ đăng nhập)
POST /login                          # PasswordSignIn + merge giỏ; ReturnUrl chỉ nhận URL nội bộ
POST /logout                         # SignOut
GET  /forgot-password                # form nhập email
POST /forgot-password                # sinh token + IEmailSender; luôn báo "đã gửi" (không lộ email tồn tại)
GET  /reset-password?token=&email=   # form đặt lại mật khẩu
POST /reset-password                 # đổi mật khẩu bằng token
GET  /account                        # [Authorize] dashboard: tabs Đơn hàng (lịch sử) + Thông tin (P2-04)
```
> ReturnUrl chống open-redirect bằng helper `UrlSafety.SafeLocalUrl`. Quên MK: `IEmailSender` hiện là
> `NullEmailSender` (chỉ log) → dev xem link reset trong log. REST `/api/auth/*` (SPEC §8) để dành cho app sau.

### Checkout (đặt hàng — MVC, COD)
`CheckoutController` (Storefront). Guest hoặc user đều đặt được. POST có antiforgery.
```
GET  /checkout            # form nhận hàng + phương thức giao + tóm tắt đơn (giỏ rỗng → 302 /cart)
POST /checkout            # tạo Order(Pending)+OrderItem(snapshot giá)+Shipment+Payment(COD); xoá giỏ → success
```
> Phase 2: chỉ **COD**, KHÔNG cổng thanh toán thật (Phase 3), KHÔNG trừ tồn kho. `Order.CustomerId` nullable
> (guest = null). Giá dòng = `IPricingService` lúc đặt (snapshot vào `OrderItem.UnitPrice`). Mã đơn hiển thị = `SD{Id:D6}`.
> SEO bắt buộc mỗi trang (xem [conventions.md](conventions.md) §SEO): `_Layout` sinh `<title>`/`description`/canonical/OG/hreflang + JSON-LD `Organization`/`WebSite`; mỗi trang bổ sung JSON-LD qua section `Head` (Category → `ItemList`+`BreadcrumbList`; PDP → `Product`+`offers`(+`aggregateRating`)+`BreadcrumbList`). Helper: `Web/Infrastructure/Seo/JsonLd`.

## Pricing
```
GET  /api/variants/{id}/price
GET  /api/products/{id}/promotions
GET  /api/products/{id}/bundle
```

## Cart & Order
```
POST   /api/cart/items
PATCH  /api/cart/items/{id}
DELETE /api/cart/items/{id}
GET    /api/cart
POST   /api/orders
GET    /api/orders/{id}
```

## Auth & Account
```
POST /api/auth/register
POST /api/auth/login
POST /api/auth/forgot-password
GET  /api/customer/info
GET  /api/customer/orders
```

## Services / Nghiệp vụ
```
POST /api/trade-in/estimate
POST /api/stock-notifications
GET  /api/stores?q=
POST /api/business/quote-request
```

## CMS
```
GET /api/articles?page=
GET /api/articles/{slug}
GET /api/pages/{slug}
```

> Quy ước response: bọc trong `ApiResponse<T>` (`success`, `data`, `error`). Phân trang trả `PagedList<T>`
> (`items`, `page`, `pageSize`, `total`).
