# ADR 0006 — Lưu trữ file ảnh qua `IFileStorage` (local disk trên API)

- Trạng thái: Accepted
- Ngày: 2026-06-19
- Liên quan: [0001](0001-clean-architecture.md), [0004](0004-admin-api-angular.md)

## Bối cảnh

Admin cần **upload ảnh sản phẩm** (gắn ở model = Product cha; storefront PDP/ProductCard đọc `ProductImages`).
Trước đây toàn bộ ảnh là placeholder. Kiến trúc tách 3 app: Admin (Angular) upload qua **API** (`HtMobile.Api`),
còn **storefront** (`HtMobile.Web`) chỉ render `<img src>` — hai app chạy ở **host khác nhau**, nên URL ảnh phải
dùng được xuyên host. Cần một nơi lưu file + cách phục vụ file.

## Quyết định

- Thêm abstraction **`IFileStorage`** (Application/Common/Interfaces): `SaveAsync(stream, fileName, folder) → đường
  dẫn tương đối` + `DeleteAsync(urlOrPath)`. Giữ Application/Domain sạch (không biết đĩa/cloud, không dính `IFormFile`).
- Hiện thực **`LocalFileStorage`** (Infrastructure): ghi xuống `{ContentRoot}/wwwroot/uploads/<folder>`, tên file
  ngẫu nhiên (`Guid`), trả `/uploads/<folder>/<file>`. API phục vụ qua `UseStaticFiles` với `PhysicalFileProvider`
  tường minh (vì `wwwroot` có thể chưa tồn tại lúc host khởi tạo → `WebRootFileProvider` null).
- **URL tuyệt đối**: controller ghép `scheme://host` của request lúc upload và lưu vào `ProductImage.Url`, để
  storefront ở host khác dùng trực tiếp (không cần cấu hình base URL → tránh lệch port dev 5097/5099).
- Đăng ký `IFileStorage` trong **`AddInfrastructure`** (dùng chung Web + API) để Web cũng phân giải được
  `AdminProductImageService` khi `ValidateOnBuild` (Development) — dù Web không dùng upload.
- Nghiệp vụ ảnh ở **`AdminProductImageService`** (Application): list / add (verify model trước khi ghi file →
  tránh file mồ côi) / delete (xoá cả file) / reorder (đặt lại `SortOrder`). Validate loại/kích thước (JPEG/PNG/
  WebP/GIF, ≤ 5MB) ở controller (nơi có `IFormFile`).

## Hệ quả

- Không cần migration: `ProductImage { ProductId, Url, SortOrder }` đã có sẵn.
- Demo/dev chạy ngay, không cần provisioning cloud. **Hạn chế**: file gắn với host API; prod cần host API có ổ
  đĩa bền/CDN, và URL tuyệt đối đã lưu sẽ "đóng băng" theo host upload → khi đổi domain phải migrate URL hoặc
  chuyển sang storage cloud. Khi đó chỉ cần thêm 1 hiện thực `IFileStorage` mới (vd `SupabaseStorage`), không đổi
  Application/Web.
- `IFileStorage` đăng ký **Singleton** (không giữ state phụ thuộc request; base URL lấy từ request tại controller).
