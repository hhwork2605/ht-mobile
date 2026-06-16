# Kiến trúc — Clean Architecture

## 4 lớp & Dependency Rule

```
┌─────────────────────────────────────────────┐
│ Web (HtMobile.Web) — ASP.NET Core MVC        │  controllers, views, viewcomponents, DI root
│ ┌─────────────────────────────────────────┐ │
│ │ Infrastructure — EF, Redis, Identity,   │ │  hiện thực interface; ra ngoài (DB, cache, 3rd-party)
│ │ ┌─────────────────────────────────────┐ │ │
│ │ │ Application — use cases / services  │ │ │  DTO, interface hạ tầng, business orchestration
│ │ │ ┌─────────────────────────────────┐ │ │ │
│ │ │ │ Domain — entities, enums, rule  │ │ │ │  KHÔNG phụ thuộc gì
│ │ │ └─────────────────────────────────┘ │ │ │
│ │ └─────────────────────────────────────┘ │ │
│ └─────────────────────────────────────────┘ │
└─────────────────────────────────────────────┘
       phụ thuộc chỉ hướng VÀO TRONG
```

## Trách nhiệm từng lớp

- **Domain** — entity (SPEC §7), enum, value object, domain event, business invariant thuần. Không EF, không attribute hạ tầng.
- **Application** — điều phối nghiệp vụ (service / Command / Query), DTO, mapping, validation. **Khai báo interface** cho mọi thứ bên ngoài (`IApplicationDbContext`, `ICacheService`, `ISearchService`, `IPricingService`, `IEmailSender`, `ICurrentUser`, `ICurrentRegion`). Chỉ ref Domain.
- **Infrastructure** — hiện thực interface của Application: `AppDbContext` (EF/Npgsql), `RedisCacheService`, `PostgresSearchService`, Identity, tích hợp Payment/TradeIn/Shipping. Chứa migrations + seed.
- **Web** — HTTP/UI: controller (Areas Storefront/Admin + Api), Razor view, ViewComponent, TagHelper, routing slug, middleware. Là *composition root*: `Program.cs` gọi `AddApplication()` + `AddInfrastructure()`.

## Vì sao đảo phụ thuộc (DIP)

Application định nghĩa `ISearchService`; Infrastructure cung cấp `PostgresSearchService`. Khi muốn đổi sang Meilisearch chỉ cần thêm 1 class mới + đổi 1 dòng DI — Application/Domain không đổi. Tương tự cache, payment, email đều thay được mà không lan toả.

## Vertical slice trong Application

Mỗi nghiệp vụ = 1 thư mục `Features/<X>` chứa `Commands/`, `Queries/`, `Dtos/`. Mặc định service-based
(class `XService`); có thể nâng lên MediatR sau mà không đổi cấu trúc thư mục.

## Pricing engine — module trung tâm

`IPricingService.GetEffectivePriceAsync(variantId, regionId)` = giá `PriceByRegion` + áp `Promotion`
đang hiệu lực (đọc `conditions_json`) + đính kèm `PaymentPromotion`. Kết quả cache Redis, invalidate
khi cập nhật giá/KM. Đây là phần được unit-test kỹ nhất (`tests/HtMobile.Application.UnitTests`).
