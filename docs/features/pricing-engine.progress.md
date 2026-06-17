# Tiến độ: Pricing engine đầy đủ — Phase 2
slug: pricing-engine
Cập nhật: 2026-06-17 01:05
Cổng hiện tại: 2

## Trạng thái cổng
- [x] 0. Thiết kế/UI      — DONE (THUẦN BACKEND, không UI — UI giá _PriceBlock/OfferList đã có)
- [x] 1. Spec            — DONE (duyệt: stack 1%+1fixed; targeting category/product/variant)
- [x] 2. Contract        — DONE (duyệt; KHÔNG đổi schema/interface)
- [x] 3. Test (red)      — DONE (15 đỏ: PromotionConditions + stacking)
- [x] 4. Implement       — DONE (build xanh, 66 test pass)
- [x] 5. Review subagent — DONE (0 Critical)
- [x] 6. Review tay       — DONE (duyệt fix H1/H2/M2/M3/L1/L3)
- [x] 7. Test (green)     — DONE (70 test pass)
- [x] 8. Commit           — IN_PROGRESS

## Đã sửa (Cổng 7)
- H1: làm tròn FinalPrice về đồng (Math.Round AwayFromZero). H2: anchor bỏ qua compareAt khi < listPrice.
- M2: tie-break ThenBy(Id). M3: +test null-element/field-thừa. L1: cập nhật comment. L3: +test >100%/làm tròn.
- Không migration (không đổi schema). Không cần smoke-test runtime (thuần logic, test dày).

## Next action
- XONG. Phase 2 hoàn tất (5/5).

## Vấn đề mở (reviewer Cổng 5)
- [H1] Chưa làm tròn tiền VND → giá hiển thị PDP có thể lệch tổng giỏ → Math.Round(finalPrice,0,AwayFromZero) + test.
- [H2] discountPercent neo compareAt khi compareAt<listPrice → bỏ qua compareAt nếu < listPrice.
- [M2] OrderByDescending(Value) thiếu tie-break → AppliedPromotionName không tất định → ThenBy(Id).
- [M3] Thiếu test conditions giá trị lạ (null phần tử/field thừa) → thêm test.
- [L1] Comment PriceCalculator lỗi thời → cập nhật. [L3] thêm test >100% + làm tròn.
- Chấp nhận: M1 cache TTL 10' (pre-existing), M4 query-all KM nhỏ, L2 hiển thị tên ghép.
- Điểm tốt: stacking đúng thứ tự + clamp; conditions OR + JSON hỏng an toàn; thuần/không regression.

## Contract (Cổng 2) — chốt
- KHÔNG đổi schema (ConditionsJson jsonb đã có) → KHÔNG migration. KHÔNG đổi IPricingService/EffectivePrice.
- App mới: PromotionConditions (thuần) — record PricingContext(variantId, productId, categoryId);
  Matches(string? json, PricingContext) parse System.Text.Json {categoryIds,productIds,variantIds}.
- PriceCalculator: nhận KM ĐÃ lọc; stacking best% → best fixed; clamp ≥0; AppliedPromotionName ghép.
- PricingEngine.ComputeAsync: nạp variant + ProductId + product.CategoryId → lọc KM qua Matches → PriceCalculator.
- Docs: data-model.md mục Promotions (schema ConditionsJson + quy tắc áp).
- [ ] 2. Contract        — TODO
- [ ] 3. Test (red)      — TODO
- [ ] 4. Implement       — TODO
- [ ] 5. Review subagent — TODO
- [ ] 6. Review tay       — TODO
- [ ] 7. Test (green)    — TODO
- [ ] 8. Commit          — TODO

## Hiện trạng pricing (gap)
- PriceCalculator: áp MỌI promotion đang hiệu lực TOÀN CỤC (bỏ qua ConditionsJson), chọn 1 KM cho giá thấp nhất.
  Gift/Voucher/Combo/BankOffer bị bỏ qua (không đổi giá).
- PricingEngine: cache Redis/in-memory TTL 10' theo CacheKeys.VariantPrice(id). Promotion có cột ConditionsJson (jsonb) nhưng CHƯA dùng.

## Quyết định cần chốt (Cổng 1)
- Targeting qua ConditionsJson: {categoryIds[],productIds[],variantIds[]} — rỗng = áp toàn bộ. (minQuantity/minSubtotal DỜI — cần ngữ cảnh số lượng).
- Mô hình "nhiều tầng": áp BEST Percentage rồi BEST FixedAmount trên giá đã giảm (tối đa 1 mỗi loại), clamp ≥ 0.
- Gift/Voucher/Combo/BankOffer: chỉ liệt kê (OfferList), KHÔNG đổi FinalPrice.
- Không đổi interface IPricingService (giữ GetEffectivePriceAsync(variantId)); cache giữ nguyên.

## Next action
- Chốt mô hình stacking + ConditionsJson scope → Cổng 2 Contract.
