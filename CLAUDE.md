# HtMobile — Apple Reseller E-commerce (ShopDunk-style)

Website TMĐT bán lẻ sản phẩm Apple (iPhone, iPad, Mac, Watch, phụ kiện, máy cũ) + hệ sinh thái
dịch vụ. Đặc tả đầy đủ: [docs/SPEC.md](docs/SPEC.md).

## Stack

- **.NET 9 / ASP.NET Core MVC** (SSR Razor) — `src/HtMobile.Web`
- **EF Core 9 + Npgsql** trên **PostgreSQL (Supabase)**
- **Redis** — cache giá/khuyến mãi + session/giỏ khách vãng lai
- **Tailwind CSS v4 + htmx + Alpine.js** — UI mobile-first, partial render
- **ASP.NET Core Identity** — auth (cookie) trên Postgres
- **PostgreSQL Full-Text Search** (tsvector + pg_trgm) — autocomplete

## Kiến trúc: Clean Architecture (Dependency Rule)

```
Web (MVC)  ──►  Infrastructure  ──►  Application  ──►  Domain
   (UI)         (EF/Redis/Identity)   (use cases)      (entities, rule)
```

Chiều phụ thuộc **chỉ hướng vào trong**. Quy tắc bắt buộc:

- `Domain` không tham chiếu project nào (pure C#).
- `Application` chỉ ref `Domain`. Khai báo **interface** cho mọi hạ tầng ở đây (`Common/Interfaces`).
- `Infrastructure` ref `Application` — hiện thực các interface (DB, Redis, search, email, payment).
- `Web` ref `Application` + `Infrastructure` (chỉ để gọi `AddInfrastructure()` ở `Program.cs`).
- `Domain` **KHÔNG** ref EF Core / ASP.NET / Redis. `Application` chỉ ref **EF Core abstractions** (`DbContext`/`DbSet` qua `IApplicationDbContext`) — **KHÔNG** ref provider Npgsql / Redis / ASP.NET. Truy cập DB luôn qua `IApplicationDbContext`.

## Bản đồ thư mục

| Đường dẫn | Vai trò |
|---|---|
| `src/HtMobile.Domain/Entities/*` | Entity gom theo nhóm (Catalog, Pricing, Sales, …) |
| `src/HtMobile.Application/Common/Interfaces/*` | Hợp đồng hạ tầng (`IApplicationDbContext`, `ICacheService`, `ISearchService`, `IPricingService`…) |
| `src/HtMobile.Application/Features/<X>/` | **Vertical slice** mỗi nghiệp vụ: `Commands/ Queries/ Dtos/` |
| `src/HtMobile.Infrastructure/Persistence/` | `AppDbContext`, `Configurations/`, `Migrations/`, `Seed/` |
| `src/HtMobile.Web/Areas/{Storefront,Admin}/` | Controllers + Views; `Api/` cho REST |
| `src/HtMobile.Web/ViewComponents/` | Component tái dùng (ProductCard, PriceBlock, …) |
| `docs/` | Nguồn sự thật: SPEC, architecture, conventions, data-model, api |
| `tasks/` | Backlog chia nhỏ theo phase (1 file = 1 đầu việc) |
| `scripts/` | `setup / dev / migrate / seed / db-reset` (.ps1 + .sh) |

## Cách thêm 1 feature (luồng chuẩn)

1. **Domain**: thêm/đổi entity trong `Domain/Entities/<Nhóm>` (+ enum nếu cần).
2. **Infrastructure/Persistence/Configurations**: thêm `IEntityTypeConfiguration<>`; thêm `DbSet` vào `IApplicationDbContext` + `AppDbContext`.
3. **Application/Features/<X>**: viết service/Command/Query + DTO; chỉ phụ thuộc interface.
4. **Web**: Controller (Area phù hợp) hoặc `Api/`; View/ViewComponent + partial cho htmx.
5. Migration: `scripts/migrate.ps1 add <Tên>` rồi `scripts/migrate.ps1 update`.
6. Test: thêm test ở `tests/HtMobile.Application.UnitTests` (nhất là pricing).

## Living Documentation — docs đi cùng code

`docs/` là **nguồn sự thật**, phải đi sát code. **Sau mỗi thay đổi code** (kể cả sửa lặt vặt
ngoài `/new-feature`), rà bảng dưới: nếu loại thay đổi khớp một dòng, **mở doc tương ứng và cập nhật
trong cùng đợt sửa**. Nếu cố ý không cập nhật (doc vẫn đúng / thay đổi thuần nội bộ) thì **nói rõ lý do**,
không lặng lẽ bỏ qua.

| Bạn vừa đổi… | Phải rà & cập nhật |
|---|---|
| Entity / field / enum, `DbSet`, migration | [docs/data-model.md](docs/data-model.md) |
| Endpoint, route/slug, DTO công khai, API contract | [docs/api.md](docs/api.md) |
| Quy ước mới (naming, pattern, cách dùng `.resx`/`Translation`/slug) | [docs/conventions.md](docs/conventions.md) |
| Kiến trúc, Dependency Rule, ranh giới project, hạ tầng mới | [docs/architecture.md](docs/architecture.md) + **ADR mới** trong [docs/decisions/](docs/decisions/) |
| Nghiệp vụ / luồng / màn hình mới | [docs/SPEC.md](docs/SPEC.md) |

Nguyên tắc: **quyết định kiến trúc lớn → ghi ADR** (`docs/decisions/NNNN-<slug>.md`), đừng chỉ sửa văn xuôi.
`/new-feature` Cổng 2 (Contract) đã ép cập nhật `data-model.md`/`api.md` cho tính năng mới — bảng này phủ
nốt phần sửa ngoài quy trình.

## Lệnh hay dùng

```powershell
scripts\setup.ps1      # restore + npm install + (gợi ý) user-secrets
scripts\dev.ps1        # chạy Web + tailwind --watch
scripts\migrate.ps1 add <Name>   # tạo migration
scripts\migrate.ps1 update       # áp migration lên DB
scripts\seed.ps1       # seed dữ liệu mẫu
dotnet build HtMobile.sln
dotnet test
```

`dotnet ef` luôn dùng: `-p src/HtMobile.Infrastructure -s src/HtMobile.Web` (DbContext ở Infrastructure, startup ở Web). `scripts/migrate.ps1` đã gói sẵn.

## Quy ước

- DB **PascalCase** (mặc định EF Core theo tên CLR; không áp naming convention). Khóa chính `long`. JSON dùng `jsonb`.
- URL SEO theo **slug** (`/iphone`, `/dien-thoai-...-256gb`) → `ISlugResolver`.
- **SEO bắt buộc khi gen UI/HTML**: HTML ngữ nghĩa (đúng 1 `<h1>` + landmark), `<title>`/`<meta description>`/`<link canonical>` riêng từng trang, **JSON-LD schema.org** (`Product`/`BreadcrumbList`/`ItemList`…), Open Graph, `alt` + `width/height` cho ảnh, `hreflang` VI/EN, và **nội dung chính phải server-render** (htmx chỉ enhance — bot không chạy JS). Chi tiết & checklist: [docs/conventions.md](docs/conventions.md) §SEO.
- Đa ngôn ngữ VI/EN: chuỗi UI qua `.resx`; nội dung DB qua entity `Translation`.
- Giá: **luôn** qua `IPricingService` (giá theo vùng + khuyến mãi), không tính tay trong controller.
- Secrets (connection string Supabase, Redis): để ở **user-secrets**, không commit. Xem [docs/setup.md](docs/setup.md).

## Lưu ý Supabase / EF

- Migrations dùng **direct connection** `db.<ref>.supabase.co:5432` (`SSL Mode=Require;Trust Server Certificate=true`).
- Runtime có thể dùng **transaction pooler** `:6543`. Hai connection string: `ConnectionStrings:Default` (runtime) + `ConnectionStrings:Migrations` (direct).
