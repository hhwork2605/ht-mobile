# ADR 0002 — Bỏ giá theo vùng (giá một mức toàn quốc)

- **Trạng thái:** Accepted (2026-06-16)
- **Bối cảnh:** Đặc tả gốc (SPEC §1, §7) thiết kế giá bán **theo khu vực** (miền Bắc / miền Nam)
  qua `Region` + `PriceByRegion`. Thực tế nghiệp vụ chỉ bán **một mức giá toàn quốc**, nên cơ chế
  giá-theo-vùng là phức tạp thừa: thêm bảng, thêm cookie/`ICurrentRegion`, thêm `RegionSelector`,
  giá phải tra theo `(variant, region)`.

## Quyết định
- **Bỏ** entity `Region` và `PriceByRegion` (cùng `DbSet`, configuration, seed).
- Giá bán nằm trực tiếp trên `ProductVariant`: `BasePrice` (giá bán) + **mới** `CompareAtPrice?`
  (giá gạch ngang) — thay cho `PriceByRegion.Price`/`CompareAtPrice`.
- Bỏ tham số `regionId` khỏi `IPricingService` (`GetEffectivePriceAsync(variantId)` / `InvalidateAsync(variantId)`),
  khỏi `EffectivePrice`, `PriceCalculator`, và toàn bộ `CatalogService`.
- Bỏ interface `ICurrentRegion` + adapter Web `CurrentRegion`, route `/set-region`, và component `RegionSelector`.
- Bỏ cột `RegionId` mồ côi ở `Store` và `Order` (không còn `Region` để trỏ tới).
- Khóa cache giá đổi `pricing:variant:{id}:region:{id}` → `pricing:variant:{id}`.
- Migration: `20260616101344_RemoveRegionPricing` (drop 2 bảng + 2 cột `RegionId`, add `ProductVariants.CompareAtPrice`).

## Hệ quả
- (+) Đơn giản hơn: ít 2 bảng, ít 1 interface, PDP không còn bộ chọn vùng.
- (+) `IPricingService` vẫn là cổng giá duy nhất → khuyến mãi (`Promotion`/`PaymentPromotion`) **giữ nguyên**.
- (−) Nếu sau này cần lại giá theo vùng phải khôi phục mô hình (đã có sẵn ở migration `Down`).

## Phương án đã loại
- Giữ `Region`/`PriceByRegion` và chỉ seed 1 vùng: vẫn kéo theo cookie/selector/tra-cứu thừa.
- Giữ `Region` cho gom nhóm cửa hàng (bỏ riêng `PriceByRegion`): không cần ở giai đoạn này; bỏ hẳn cho gọn.
