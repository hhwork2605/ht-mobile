# P5-02 Admin — Quản lý sản phẩm

- **Phase:** 5
- **Tính năng SPEC:** §4.3 (Admin: quản lý danh mục/sản phẩm/biến thể), §7
- **Trạng thái:** doing

## Mục tiêu
Admin xem danh sách sản phẩm (lọc theo danh mục), **thêm/sửa** sản phẩm cùng biến thể, **ẩn/hiện** (đổi trạng thái
biến thể). Trong Area Admin, chỉ role Admin.

## Phạm vi (in-scope)
- [ ] `GET /admin/products?category=` : bảng SP (ảnh/tên, danh mục, SKU đại diện, số biến thể, khoảng giá, trạng thái)
      + lọc theo danh mục (dropdown từ Categories). Mới nhất trước.
- [ ] `GET /admin/products/create` + `POST` : tạo SP (Name, Slug auto-gen từ Name nếu bỏ trống, CategoryId, Brand,
      Tagline, Description) + **1 biến thể đầu** (Sku, Storage, Color, BasePrice, CompareAtPrice?, Status). Slug unique.
- [ ] `GET /admin/products/{id}/edit` + `POST` : sửa field SP + danh sách biến thể (sửa BasePrice/CompareAt/Status,
      thêm biến thể mới). Ảnh: 1 ô URL (mặc định /images/placeholder.svg) — KHÔNG upload.
- [ ] `POST /admin/products/variants/{id}/toggle` : ẩn/hiện biến thể (Active ⇄ Discontinued).
- [ ] Helper thuần `Slugify.ToSlug(name)`: tiếng Việt → ascii, thường, gạch nối. Có test.
- [ ] Tất cả `[Authorize(Roles = Admin)]`; antiforgery; giá là decimal ≥ 0 (validate).
- [ ] **Admin không tối ưu SEO** (noindex, không slug/canonical/JSON-LD). **Dùng AJAX (htmx)** cho thêm biến thể /
      đổi trạng thái / lưu nhanh: POST trả partial cập nhật tại chỗ (không reload). (Tạo/sửa SP có thể dùng form
      thường redirect cho đơn giản; thao tác trên-bảng/biến thể dùng htmx.) Xem conventions §Admin.

## Ngoài phạm vi (out-of-scope)
- Tồn kho theo cửa hàng / ma trận tồn (cần Inventory) — feature riêng.
- Upload ảnh / nhiều ảnh / gallery (chỉ 1 URL). Rich text mô tả. Xoá cứng SP (chỉ ẩn qua Status).
- Quản lý danh mục (CRUD category) — feature riêng; ở đây chỉ chọn danh mục có sẵn.
- Cây danh mục lồng nhau ở sidebar (dùng dropdown phẳng).

## File sẽ đụng
- `src/HtMobile.Application/Common/Slugify.cs` (thuần) hoặc Features/Catalog.
- `src/HtMobile.Application/Features/Catalog/AdminProductService.cs` + Dtos (list/create/edit).
- `src/HtMobile.Web/Areas/Admin/Controllers/ProductsController.cs` + Views (Index, Create, Edit) + ViewModels.
- `src/HtMobile.Web/Areas/Admin/Views/Shared/_Layout.cshtml` (nav "Sản phẩm" → active, bỏ disabled).
- `tests/HtMobile.Application.UnitTests/Catalog/SlugifyTests.cs`.

## Phụ thuộc
- Product/ProductVariant/ProductImage/Category (đã có), Admin layout (P5-01), Identity Admin. KHÔNG entity/migration mới.

## Tiêu chí done
- [ ] Build xanh, Dependency Rule OK (DB qua IApplicationDbContext; Slugify thuần).
- [ ] /admin/products liệt kê + lọc danh mục; tạo SP mới → hiện ở list + storefront; sửa giá/field → phản ánh.
- [ ] Ẩn biến thể (Discontinued) → PDP không cho mua (đã có nhánh !InStock). Slug trùng → báo lỗi, không tạo.
- [ ] Non-admin → chặn. Giá âm/thiếu field → validate.
- [ ] Unit test Slugify (tiếng Việt có dấu, khoảng trắng, ký tự đặc biệt, rỗng).

## Cách verify
- Login admin → /admin/products → Thêm SP (vd "Tai nghe ABC") → thấy ở list + mở slug ở storefront.
- `dotnet test` cho Slugify.
