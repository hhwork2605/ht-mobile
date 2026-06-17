# Tiến độ: Admin — Quản lý sản phẩm — Phase 5
slug: admin-products
Cập nhật: 2026-06-17 08:00
Cổng hiện tại: 8

## Trạng thái cổng
- [x] 0. Thiết kế/UI      — DONE (màn Sản phẩm ShopDunk Admin; form thêm/sửa dựng theo style admin)
- [x] 1. Spec            — DONE (duyệt)
- [x] 2. Contract        — DONE (duyệt)
- [x] 3. Test (red)      — DONE (SlugifyTests 9 fail: NotImplementedException)
- [x] 4. Implement       — DONE (build xanh, 98 app + 18 integ + 24 func test xanh)
- [x] 5. Review subagent — DONE (xem mục dưới)
- [x] 6. Review tay       — DONE (duyệt) + smoke-test runtime OK (login→tạo SP→AJAX thêm/ẩn biến thể→PDP)
- [x] 7. Test (green)    — DONE (build 0 warning; 98 app + 25 integ + 24 func xanh)
- [ ] 8. Commit          — IN_PROGRESS (commit, không push)

## Artifact đã tạo
- src/HtMobile.Application/Common/Slugify.cs (impl: Normalize FormD bỏ dấu + đ→d + [a-z0-9]→gạch nối)
- tests/.../Catalog/SlugifyTests.cs (9 case)
- src/HtMobile.Application/Features/Catalog/Dtos/AdminProductDtos.cs (rows/edit/input records + AdminProductResult)
- src/HtMobile.Application/Features/Catalog/AdminProductService.cs (List/CategoryOptions/Edit/Create/Update/AddVariant/ToggleVariant)
- DependencyInjection.cs: + AddScoped<AdminProductService>
- src/HtMobile.Web/Areas/Admin/Models/ProductViewModels.cs (Create/Edit/VariantEdit/VariantAdd VM + validate giá ≥0)
- src/HtMobile.Web/Areas/Admin/Controllers/ProductsController.cs
- Views/Products: Index, Create, Edit, _VariantRow (partial)
- Admin _Layout: + htmx + xsrf meta + configRequest; nav "Sản phẩm" active
- _ViewImports: + Catalog namespaces + Areas.Admin.Models
- docs/api.md: + route toggle; docs/conventions.md §Admin (đã có)

## Quyết định chính
- Slug auto-gen từ Name nếu trống; unique check (Create + Update đổi slug). Slug biến thể = slug SP + storage/color/sku.
- AJAX (htmx) cho thêm biến thể (append _VariantRow) + ẩn/hiện (toggle → swap _VariantRow). Form chính (field SP + giá/status
  biến thể) POST redirect cho đơn giản. _VariantRow render name="Variants[i].*" để bind lại vào form chính.
- Toggle Active⇄Discontinued; render lại giá từ DB → bỏ chỉnh giá chưa lưu ở hàng đó (chấp nhận với admin).
- ToggleVariant/AddVariant đụng TIỀN gián tiếp (set/đổi trạng thái bán) → nêu cờ Cổng 6.

## Review Cổng 5 (code-reviewer) + xử lý
- [ĐÃ SỬA] Critical: trùng SKU/slug biến thể → 500. Thêm VariantConflictAsync; Create/AddVariant trả SkuExists; controller báo lỗi.
- [ĐÃ SỬA] Minor: CompareAtPrice < BasePrice. Thêm IValidatableObject (PriceRule) cho Create/Edit/AddVariant VM.
- [ĐÃ SỬA] Minor: toggle tin productId client → ToggleVariantAsync trả ProductId từ DB; bỏ hx-vals.
- [ĐÃ SỬA] Nit: InvariantCulture cho input number ở _VariantRow; bỏ ProductId thừa khỏi VariantAddVm.
- [ĐÃ THÊM] Test: AdminProductServiceTests (7 case) ở Infrastructure.IntegrationTests/Catalog.
- [GHI NHẬN, ngoài scope] Major #2 index binding chỉ rủi ro khi thêm nhiều biến thể trên trang cũ/đa người dùng;
  luồng 1 người tuần tự đúng (index = DB count-1 khớp số hàng render). Đổi slug SP không 301-redirect (ngoài scope P5-02).

## Next action
- Cổng 6: trình review tay (cờ: set giá BasePrice/CompareAt, quyền Admin). DỪNG chờ duyệt → Cổng 7/8 + smoke-test.
