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
