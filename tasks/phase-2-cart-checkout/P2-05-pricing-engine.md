# P2-05 Pricing engine đầy đủ

- **Phase:** 2
- **Tính năng SPEC:** §2 (pricing engine), §4.2 (khuyến mãi nhiều tầng), [ADR-0002] (giá một mức toàn quốc)
- **Trạng thái:** doing

## Mục tiêu
Nâng pricing từ "áp mọi KM toàn cục, chọn 1 cái" → **nhắm mục tiêu theo `ConditionsJson`** (danh mục/SP/variant)
và **áp nhiều tầng** (1 % + 1 số tiền) đúng đắn, vẫn qua `IPricingService` + cache.

## Phạm vi (in-scope)
- [ ] Parse + match `ConditionsJson`: `{ "categoryIds":[], "productIds":[], "variantIds":[] }` (đều optional;
      rỗng/null = áp toàn bộ). KM chỉ áp khi variant khớp ≥1 danh sách được khai (hoặc không khai danh sách nào).
- [ ] Logic thuần (test): `PromotionConditions.Matches(json, ctx)` với ctx = (variantId, productId, categoryId).
- [ ] **Nhiều tầng**: áp **Percentage tốt nhất** (giảm % lớn nhất) → rồi **FixedAmount tốt nhất** trên giá đã giảm
      (tối đa 1 mỗi loại). `FinalPrice` clamp ≥ 0. `AppliedPromotionName` = ghép tên các KM đã áp.
- [ ] Gift/Voucher/Combo/BankOffer: KHÔNG đổi `FinalPrice` (chỉ là ưu đãi liệt kê ở OfferList).
- [ ] `PricingEngine` nạp thêm `ProductId`/`CategoryId` của variant để lọc KM theo điều kiện; cache giữ nguyên.
- [ ] `DiscountPercent` tính lại theo mốc cao nhất (CompareAt/List) so với FinalPrice (như cũ).

## Ngoài phạm vi (out-of-scope)
- Điều kiện theo **số lượng/min đơn** (`minQuantity`, `minSubtotal`) — cần ngữ cảnh giỏ/qty; DỜI (GetEffectivePriceAsync chỉ theo variant).
- Voucher nhập tay (mã giảm giá ở giỏ), ưu đãi ngân hàng tính tiền, combo bó sản phẩm → Phase sau.
- Invalidate cache khi Admin sửa KM (Admin chưa có) — thêm hook khi làm Admin (Phase 5).
- Đổi `IPricingService` / signature / UI.

## File sẽ đụng
- `src/HtMobile.Application/Features/Pricing/PromotionConditions.cs` (mới, thuần) — parse + match ConditionsJson.
- `src/HtMobile.Application/Features/Pricing/PriceCalculator.cs` — stacking %+fixed; nhận KM đã lọc.
- `src/HtMobile.Application/Features/Pricing/PricingEngine.cs` — nạp ProductId/CategoryId + lọc KM theo điều kiện.
- `tests/HtMobile.Application.UnitTests/Pricing/PromotionConditionsTests.cs` + bổ sung `PriceCalculatorTests` (stacking).

## Phụ thuộc
- `Promotion.ConditionsJson` (jsonb, đã có), `IPricingService`/`PricingEngine`/cache (đã có), `EffectivePrice` (đã có).

## Tiêu chí done
- [ ] Build xanh, Dependency Rule OK (Application không ref Npgsql; parse JSON bằng System.Text.Json).
- [ ] KM có `categoryIds/productIds/variantIds` chỉ áp cho variant khớp; KM rỗng điều kiện áp toàn bộ.
- [ ] Áp 1 % + 1 fixed đúng thứ tự (%, rồi fixed) cho giá thấp nhất; clamp ≥ 0; tên KM ghép.
- [ ] Gift/Voucher/Combo/BankOffer không đổi giá.
- [ ] Unit test: matcher (khớp/không khớp theo từng loại danh sách + rỗng) + stacking (%, fixed, cả hai, clamp).

## Cách verify
- `dotnet test` (PromotionConditions + PriceCalculator).
- Smoke (tuỳ chọn): seed KM có ConditionsJson theo 1 danh mục → PDP trong danh mục đó giảm giá, ngoài danh mục thì không.
