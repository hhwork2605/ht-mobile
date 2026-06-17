# 0004 — Tách Web API riêng (HtMobile.Api) + admin SPA Angular (HtMobile.Admin)

- **Trạng thái:** Accepted
- **Ngày:** 2026-06-17
- **Liên quan:** [0001-clean-architecture](0001-clean-architecture.md), [conventions.md](../conventions.md)

## Bối cảnh
App MVC `HtMobile.Web` chỉ phục vụ storefront (ADR/quyết định trước). Khu quản trị cần tách thành SPA riêng
(Angular) để linh hoạt, gọi qua REST API. Backend tái dùng tầng `Application`/`Infrastructure` sẵn có.

## Quyết định
1. **`src/HtMobile.Api`** — ASP.NET Core Web API (.NET 9, controllers). Tham chiếu `Application` + `Infrastructure`
   (Clean Architecture giữ nguyên: API chỉ là host mới). Không trùng route với storefront.
2. **Xác thực JWT bearer**: `POST /api/auth/login` xác thực qua Identity (`UserManager.CheckPasswordAsync`),
   chỉ cấp token cho tài khoản **role Admin**. Token ký HMAC-SHA256, claim `sub/email/name/role`.
   Controllers gắn `[Authorize(AuthenticationSchemes = "Bearer", Roles = Admin)]` (Identity cookie vẫn tồn tại
   do `AddInfrastructure` nhưng API dùng scheme Bearer tường minh). Cấu hình ở section `Jwt` (key để user-secrets khi deploy).
3. **CORS** policy `HtAdmin` cho phép origin Angular (`Cors:AdminOrigins`, dev = `http://localhost:4200`).
4. **API endpoints (đợt 1 — Sản phẩm)**: `GET/POST/PUT /api/products`, `/api/products/{id}/variants`,
   `/api/products/variants/{id}/toggle`, `GET /api/categories`, `/api/auth/{login,me}`. Dùng lại `AdminProductService`
   (đã khôi phục) trên mô hình Product cha–con + ProductAttribute (ADR 0003).
5. **`HtMobile.Admin`** — Angular 19 (standalone) + **PrimeNG 19** (preset Aura, primary đổi sang brand `#0070F4`
   theo design ShopDunk Admin). Auth: JWT lưu localStorage + HTTP interceptor gắn Bearer + route guard. Trang: Login,
   shell (sidebar 244px + header theo design), Products (list PrimeNG Table + lọc danh mục), Product form (tạo/sửa +
   bảng biến thể: sửa giá/ẩn-hiện/thêm biến thể). `environment.apiBase` trỏ tới API.

## Hệ quả
- 2 host .NET (`HtMobile.Web` storefront cổng riêng, `HtMobile.Api` cổng riêng) + 1 SPA Angular, **chung 1 DB**
  (Supabase) + chung `Application`/`Infrastructure`.
- Identity admin user/role (seed ở `DbInitializer` của storefront) dùng chung; API chỉ đọc/verify.
- Migration/seed vẫn do storefront chạy lúc dev; API không tự migrate.
- `Jwt:Key` để placeholder ở `appsettings` dev — **phải chuyển user-secrets/KeyVault khi deploy thật** (cùng vấn đề
  với connection string Supabase đang nằm trong appsettings — nợ kỹ thuật bảo mật cần xử lý trước production).

## Phương án đã loại
- **API nhúng trong HtMobile.Web**: loại — muốn tách host/triển khai độc lập với storefront.
- **Cookie auth cho SPA**: loại — JWT stateless hợp SPA tách domain hơn, tránh vướng CSRF/cookie cross-site.
