# API contract (REST — nguồn gốc SPEC §8)

Controller đặt ở `src/HtMobile.Web/Api`, attribute-routed `[Route("api/...")]`, trả JSON.
Trang khách (SSR) gọi service Application trực tiếp; API phục vụ autocomplete/htmx/đối tác.

## Catalog
```
GET  /api/categories
GET  /api/categories/{slug}/products?series=&page=&sort=
GET  /api/products/{slug}                 # PDP + variants + media + reviews
GET  /api/variants/{slug}?region={id}     # giá theo vùng
GET  /api/search/suggest?q=               # autocomplete (keyword + products)
```

## Pricing
```
GET  /api/variants/{id}/price?region={id}
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
GET  /api/stores?region=&q=
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
