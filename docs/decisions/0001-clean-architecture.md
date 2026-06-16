# ADR 0001 — Clean Architecture + Modular Monolith

- **Trạng thái:** Accepted (2026-06-16)
- **Bối cảnh:** Cần cấu trúc cho dự án TMĐT Apple (SPEC) sao cho dễ "vibe-code" từng tính năng, tách bạch nghiệp vụ khỏi hạ tầng, dễ test và dễ thay thành phần (search/payment).

## Quyết định
- Dùng **Clean Architecture** 4 lớp (Domain ← Application ← Infrastructure ← Web), triển khai dưới dạng **modular monolith** (1 app, deploy 1 lần).
- Auth: **ASP.NET Core Identity** trên Postgres (không dùng Supabase Auth) để idiomatic với MVC.
- UI: **ASP.NET Core MVC SSR + Tailwind v4 + htmx + Alpine** (SEO + nhẹ JS).
- Search: **PostgreSQL FTS** ẩn sau `ISearchService` (nâng cấp Meilisearch sau nếu cần).

## Hệ quả
- (+) Ranh giới rõ, mỗi feature có "nhà" cố định → vibe-code nhanh.
- (+) Đổi hạ tầng = đổi 1 implementation, không lan toả.
- (−) Nhiều project/boilerplate hơn single-project; chấp nhận để đổi lấy khả năng mở rộng.

## Phương án đã loại
- Tách Web API + MVC frontend riêng: đúng sơ đồ SPEC nhưng nhiều overhead vận hành cho giai đoạn đầu.
- Single project: nhanh lúc đầu nhưng rối khi feature tăng.
