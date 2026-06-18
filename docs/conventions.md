# Quy ước code

## Đặt tên & cấu trúc
- Namespace theo thư mục: `HtMobile.Domain.Entities.Catalog`, `HtMobile.Application.Features.Catalog`…
- Entity: danh từ số ít (`Product`, `ProductVariant`). DbSet: số nhiều (`Products`).
- DTO hậu tố `Dto` (`ProductDetailDto`); Command/Query hậu tố `Command`/`Query`.
- ViewComponent: `XViewComponent` + View `Components/X/Default.cshtml`.

## Database (PostgreSQL / Supabase)
- **PascalCase** toàn bộ bảng/cột — mặc định EF Core (theo tên CLR), **không** áp naming convention (đã bỏ `UseSnakeCaseNamingConvention()`). Định danh được EF trích dẫn (`"Products"`, `"CreatedAt"`).
- Khóa chính `long` (bigint identity). FK cùng kiểu `long`.
- Cột JSON (`SpecsJson`, `ConditionsJson`) map sang **`jsonb`**.
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
- **Design storefront = "ShopDunk Storefront"** (apple theme). Token màu/bo góc khai báo ở `:root` trong
  `Views/Shared/_Layout.cshtml` (`--accent`, `--accent-press`, `--sale` = `#FF424E`, `--ink`/`--ink2`,
  `--bg`/`--bg2`, `--line`, `--header*`, `--radius-card`, `--success`/`--success-50`) và map vào `tailwind.config`
  (class `accent`, `sale`, `ink`, `surface`/`surface2`, `line`, `rounded-card`…). Dùng các class này thay vì màu Tailwind cứng (`text-red-600`…).
- **Icon = KV Icon Kit (self-host)**, KHÔNG dùng Font Awesome. CSS ở `wwwroot/css/kv-icons.css`, font ở `wwwroot/fonts/`.
  Markup: `<i class="iks ik-...">` (solid), `<i class="ikr ik-...">` (regular), `<i class="ikb ik-...">` (brands).
  Map slug danh mục → glyph qua `HtMobile.Web.Infrastructure.StorefrontIcons.Category(slug)`.

## Admin — KHÔNG nằm trong app MVC này
App MVC `HtMobile.Web` **chỉ phục vụ bán hàng (storefront)**. Khu quản trị (admin) sẽ là **một webapp Angular riêng**
(repo/khác), dùng REST API sau này — **không** dựng trang admin bằng MVC/Razor trong project này nữa. Identity + role
`Admin` (seed ở `DbInitializer`) giữ lại làm hạ tầng auth cho API tương lai.

## SEO (bắt buộc khi gen trang/HTML)
Mọi trang SSR phải render chuẩn SEO **ngay khi sinh code** — đích: rich result + Core Web Vitals tốt, dễ lên top.

- **HTML ngữ nghĩa**: dùng landmark `<header> <nav> <main> <article> <section> <footer>`; **đúng 1 `<h1>` mỗi trang**, phân cấp `h1 → h2 → h3` hợp lý; điều hướng bằng `<a href>` thật (không `<div onclick>`).
- **Thẻ `<head>` theo từng trang** (qua `ViewData`/section trong `_Layout`): `<title>` duy nhất (≤ 60 ký tự) + `<meta name="description">` (≤ 160) + `<link rel="canonical">`. PDP canonical về **1 URL chuẩn** để tránh trùng nội dung do nhiều variant slug. Thêm Open Graph (`og:title/description/image/type/url`) + Twitter Card.
- **Structured data JSON-LD (schema.org)** theo loại trang:
  - PDP → `Product` + `offers` (`priceCurrency:"VND"`, `price`, `availability`) + `aggregateRating` (nếu có review).
  - Trang danh mục → `ItemList`; **mọi trang** → `BreadcrumbList`; `_Layout` → `Organization` + `WebSite` (kèm `SearchAction`).
  - Bài viết → `Article`.
- **Ảnh**: `alt` mô tả thật; **luôn set `width`/`height`** (chống CLS); `loading="lazy"` cho ảnh dưới màn đầu; URL/tên file có nghĩa.
- **Đa ngôn ngữ**: `<html lang="...">` đúng + `<link rel="alternate" hreflang>` cho VI/EN.
- **Crawlable**: nội dung chính phải nằm trong **HTML server-render ban đầu**; htmx chỉ để *enhance* (gợi ý, lọc, phân trang), **không** dùng để render nội dung chính (bot không chạy JS). Trang thiếu trả đúng **404**; có `robots.txt` + `sitemap.xml`; internal link có anchor mô tả.
- **Core Web Vitals**: SSR nhanh, tránh layout shift (khung/ảnh có kích thước cố định), defer JS không thiết yếu, mobile-first.
- URL theo **slug** sạch (đã có ở mục Web/UI) — tránh tham số rác, không lộ id.

## Đa ngôn ngữ
- Chuỗi UI: `.resx` trong `Web/Resources` + `IStringLocalizer`.
- Nội dung động (tên SP, mô tả): entity `Translation(entity, entity_id, lang, field, value)`.

## Bảo mật (SPEC §13)
- Mật khẩu qua ASP.NET Identity (PBKDF2 mặc định). HTTPS bắt buộc.
- Không lưu dữ liệu thẻ — uỷ thác cổng thanh toán.
- Validate + sanitize input; chống CSRF (antiforgery) / XSS; rate-limit login/search/checkout.

## Git
- Branch theo task: `feat/phase1-catalog-grid`. Commit nhỏ, gắn id task trong `tasks/`.
