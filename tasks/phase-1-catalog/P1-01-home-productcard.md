# P1-01 — ProductCard ViewComponent + Trang chủ

- **Phase:** 1 (Catalog & tìm kiếm)
- **Tính năng SPEC:** §4.1 (ProductCard) + §5/§9 (Trang chủ, layout storefront)
- **Trạng thái:** done
- **Thiết kế:** ShopDunk Storefront (Claude Design bundle) — theme **Apple-clean**, token qua CSS variables.

## Mục tiêu
Dựng **ProductCard tái dùng** (ViewComponent) và **Trang chủ** đúng thiết kế Apple-clean, làm nền (theme
tokens + chrome + thẻ SP) cho các màn Phase 1 còn lại (Danh mục, PDP, Search).

## Phạm vi (in-scope)
- [ ] **Theme tokens nền**: khai báo CSS variables (`--accent, --accent-press, --sale, --ink, --ink2, --bg,
      --bg2, --line, --header, --header-ink, --header-border, --radius-card`) — preset **apple** mặc định —
      và map vào Tailwind để dùng `bg-accent / text-ink / text-ink2 / bg-surface / bg-surface2 / border-line /
      text-sale / bg-header / rounded-card`. (Đặt nền cho việc đổi theme sau, đúng yêu cầu chat gốc.)
- [ ] **Icon: Font Awesome Free** (thay KV Icon Kit của bundle) — nạp bộ free (solid/brands) và map các icon
      dùng trong thiết kế (giỏ, người dùng, tìm kiếm, truck, shield-check, credit-card, apple, social…).
- [ ] **ProductCard ViewComponent** (`ProductCardViewComponent` + `Components/ProductCard/Default.cshtml`):
      ảnh/placeholder (badge góc trái: `-N%` khi có giảm, và/hoặc nhãn "Mới"), tên, **tagline**, giá (gạch giá
      cũ khi có), nút **"Mua"**; hover nhấc + đổ bóng. Thay cách dùng partial `_ProductCard.cshtml` hiện tại.
- [ ] **Trang chủ** dựng lại theo thiết kế: **banner slider** (carousel nhiều slide: ảnh + eyebrow + tiêu đề
      + 2 CTA, tự chạy + chấm điều hướng/prev-next, bằng Alpine.js) › **hàng tile danh mục** › lưới
      **"Sản phẩm nổi bật"** (4 cột desktop / 2 cột mobile) › **2 bento** (Trả góp 0% / Thu cũ đổi mới) ›
      **trust bar** 4 mục (giao nhanh / chính hãng / trả góp / đổi trả).
      - *Nguồn dữ liệu banner cần chốt ở Cổng 2:* thêm entity `Banner` (Domain + migration + seed) **hay** hardcode tạm.
- [ ] **Chrome chung (layout)** restyle Apple-clean: thanh thông báo trên cùng, header (logo + nav danh mục +
      ô tìm kiếm giữ htmx hiện có + icon tài khoản + icon giỏ kèm badge), footer 3 cột + social. Responsive
      mobile-first (header mobile gọn).
- [ ] Giá **luôn qua `IPricingService`** (đã có sẵn trong `CatalogService`). URL theo **slug**.

## Ngoài phạm vi (out-of-scope)
- Trang Danh mục (PLP), PDP, Search, Giỏ/Checkout/Tài khoản → các slice/phase sau.
- Hành vi **"Mua"/thêm giỏ thật** (Phase 2) — nút "Mua" tạm điều hướng tới SP hoặc bắn toast, **không** ghi giỏ.
- **Khối SP theo từng danh mục** ở trang chủ (code `GetHomeBlocksAsync` hiện tại) → thiết kế dùng **1 lưới
  "nổi bật"** + tile danh mục; sẽ đổi read-model (chi tiết ở Cổng 2).
- **Theme switcher cho người dùng** (dropdown THEME chỉ là chrome của tool design) — chỉ set token, không có UI đổi theme.
- Mobile **bottom nav** đầy đủ (có tab Giỏ/Tài khoản thuộc Phase 2) — để slice sau.
- Quản trị banner (Admin CRUD) — Phase 5; slice này chỉ hiển thị + (nếu chốt) entity/seed.

## File sẽ đụng (dự kiến — chốt ở Cổng 2)
- `src/HtMobile.Web/ViewComponents/ProductCardViewComponent.cs` (mới)
- `src/HtMobile.Web/Views/Shared/Components/ProductCard/Default.cshtml` (mới)
- `src/HtMobile.Web/Areas/Storefront/Views/Home/Index.cshtml` (viết lại)
- `src/HtMobile.Web/Areas/Storefront/Controllers/HomeController.cs` (đổi theo read-model mới)
- `src/HtMobile.Web/Views/Shared/_Layout.cshtml` + `Components/Menu/Default.cshtml` (restyle header/footer/nav)
- `src/HtMobile.Web/Styles/app.css` (theme tokens) + cấu hình Tailwind
- `src/HtMobile.Application/Features/Catalog/CatalogService.cs` + `Dtos/CatalogDtos.cs`
  (read-model trang chủ: featured + categories; thêm `Tagline`/`Badge`/`IsNew` cho `ProductCardDto` nếu chốt)

## Phụ thuộc
- Scaffold Phase 0 (Domain/`IApplicationDbContext`/`IPricingService`/`CatalogService`) — đã có.
- Cần seed dữ liệu mẫu (`scripts/seed.ps1`) để trang chủ có sản phẩm hiển thị.

## Tiêu chí done
- [ ] `dotnet build` xanh, không vi phạm Dependency Rule (Web không gọi DB trực tiếp; giá qua `IPricingService`).
- [ ] Trang `/` hiển thị đúng bố cục thiết kế: hero, tile danh mục, lưới nổi bật, 2 bento, trust bar, header/footer.
- [ ] ProductCard là **ViewComponent tái dùng**, render đúng badge/giá/giá-gạch/tagline/nút Mua.
- [ ] Responsive: desktop 4 cột, mobile 2 cột; header thu gọn ở mobile.
- [ ] Đổi `--accent` (CSS var) làm đổi tông màu toàn trang (chứng minh tokens hoạt động).

## Contract (chốt Cổng 2)
- **Domain**: `Banner` (`Entities/Cms/Banner.cs`): `Eyebrow?, Title, Subtitle?, ImageUrl?, LinkUrl?, CtaText?,
  SortOrder, IsActive, StartsAt?, EndsAt?` (kế thừa `BaseAuditableEntity`). Thêm `Product.Tagline` (nullable).
- **Infrastructure**: `BannerConfiguration`; `DbSet<Banner> Banners` ở `IApplicationDbContext` + `AppDbContext`;
  migration `AddBannerAndProductTagline`; seed banner + bổ sung sản phẩm + tagline.
- **Application**: `BannerDto(Eyebrow,Title,Subtitle,ImageUrl,LinkUrl,CtaText)`; `HomePageDto{Banners,Categories,Featured}`;
  `CatalogService.GetHomePageAsync(regionId,featuredCount=8,ct)`; `ProductCardDto` + `Tagline` + `IsNew`.
  Logic thuần tách ra để test: **banner đang hiệu lực** (`IsActive` + cửa sổ `StartsAt/EndsAt`) và **IsNew** (theo `CreatedAt`).
- **Web**: `ProductCardViewComponent` + `Components/ProductCard/Default.cshtml`; viết lại `Home/Index.cshtml` +
  `HomeController`; restyle `_Layout` + `Components/Menu/Default.cshtml`; theme tokens trong `app.css`;
  Font Awesome Free; chuyển dev sang dùng app.css đã build.
- **Icon**: Font Awesome Free (solid + brands).
- **API**: không thêm endpoint → `docs/api.md` giữ nguyên. `docs/data-model.md` cập nhật (Banner + Product.Tagline) ở Cổng 4.

## Cách verify
- `scripts/dev.ps1` → mở `/` xem bố cục + responsive (thu nhỏ cửa sổ).
- Tạm sửa `--accent` trong `app.css` sang màu khác → reload thấy nút/eyebrow/link đổi màu.
- (Không có logic nghiệp vụ thuần mới ở slice này; test đơn vị sẽ tập trung ở slice Pricing/PDP.)
