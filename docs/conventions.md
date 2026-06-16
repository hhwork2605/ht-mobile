# Quy ước code

## Đặt tên & cấu trúc
- Namespace theo thư mục: `HtMobile.Domain.Entities.Catalog`, `HtMobile.Application.Features.Catalog`…
- Entity: danh từ số ít (`Product`, `ProductVariant`). DbSet: số nhiều (`Products`).
- DTO hậu tố `Dto` (`ProductDetailDto`); Command/Query hậu tố `Command`/`Query`.
- ViewComponent: `XViewComponent` + View `Components/X/Default.cshtml`.

## Database (PostgreSQL / Supabase)
- **snake_case** toàn bộ bảng/cột — tự động qua `EFCore.NamingConventions` (cấu hình ở `AppDbContext`).
- Khóa chính `long` (bigint identity). FK cùng kiểu `long`.
- Cột JSON (`specs_json`, `conditions_json`) map sang **`jsonb`**.
- Tiền tệ: `decimal(18,2)`. Thời gian: `DateTime` **giờ server** (`DateTime.Now`, qua `IDateTime`) → cột `timestamp` (without time zone); set qua interceptor `CreatedAt/UpdatedAt`. Bật `Npgsql.EnableLegacyTimestampBehavior` ở `AddInfrastructure` để map `DateTime`↔`timestamp` và bỏ kiểm tra `DateTimeKind`. (Riêng `lockout_end` của Identity vẫn là `timestamptz` do framework.)
- Mỗi entity có 1 `IEntityTypeConfiguration<>` riêng trong `Infrastructure/Persistence/Configurations`.

## Truy cập dữ liệu
- Application **chỉ** dùng `IApplicationDbContext` (không `AppDbContext` trực tiếp).
- Đọc danh sách: trả `PagedList<T>`; kết quả: `Result<T>` (tránh ném exception cho luồng nghiệp vụ thường).

## Giá & khuyến mãi
- **Luôn** lấy giá qua `IPricingService`. Không tự cộng/trừ trong controller/view.
- Cache key chuẩn ở `Domain/Constants/CacheKeys`.

## Web / UI
- Mobile-first, Tailwind utility. Component tái dùng = ViewComponent (SPEC §9).
- Tương tác động: htmx trả **partial view**; state nhỏ dùng Alpine. Hạn chế JS rời rạc.
- URL khách = slug (không lộ id). Resolve qua `ISlugResolver`.

## Đa ngôn ngữ
- Chuỗi UI: `.resx` trong `Web/Resources` + `IStringLocalizer`.
- Nội dung động (tên SP, mô tả): entity `Translation(entity, entity_id, lang, field, value)`.

## Bảo mật (SPEC §13)
- Mật khẩu qua ASP.NET Identity (PBKDF2 mặc định). HTTPS bắt buộc.
- Không lưu dữ liệu thẻ — uỷ thác cổng thanh toán.
- Validate + sanitize input; chống CSRF (antiforgery) / XSS; rate-limit login/search/checkout.

## Git
- Branch theo task: `feat/phase1-catalog-grid`. Commit nhỏ, gắn id task trong `tasks/`.
