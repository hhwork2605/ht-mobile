# Tiến độ: Thu cũ đổi mới — Phase 3
slug: trade-in
Cập nhật: 2026-06-17 02:35
Cổng hiện tại: 1

## Trạng thái cổng
- [x] 0. Thiết kế/UI      — DONE (tự dựng theo SPEC §6 UC-04 + tokens)
- [x] 1. Spec            — DONE (duyệt)
- [x] 2. Contract        — DONE (duyệt; không schema/migration)
- [x] 3. Test (red)      — DONE (2 estimator đỏ)
- [x] 4. Implement       — DONE (build xanh, 82 test pass)
- [x] 5. Review subagent — DONE (Đạt, 0 blocker)
- [x] 6. Review tay       — DONE (duyệt: +test TradeInService, comment EstimatedPrice)
- [x] 7. Test (green)     — DONE (86 test pass)
- [x] 8. Commit           — IN_PROGRESS

## Next action
- XONG. Phase 3: 3/5 (Bundle, Trả góp, Thu cũ). Còn Theo dõi hàng về, Cổng thanh toán.

## Vấn đề mở (reviewer Cổng 5)
- [M1] Chưa test TradeInService → thêm integration test (key lạ → null + không tạo request; EstimatedPrice = quote server).
- [M2] EstimatedPrice = Total (gồm trợ giá) → thêm comment làm rõ ngữ nghĩa.
- [L] Banner "đến 3 triệu" vs subsidy 500k (marketing "đến X" — chấp nhận). Nhánh chết tradeIn<0 (defensive — giữ).
  JS half-even vs server AwayFromZero (server tính lại khi submit — chấp nhận).
- Điểm tốt: estimate server-side; từ chối key lạ; antiforgery; guest OK; thuần/Dependency Rule; noindex; 1 h1.

## Contract (Cổng 2) — chốt
- KHÔNG entity/DB/migration mới (TradeInRequest đã có).
- App Features/TradeIn:
  - TradeInEstimator (thuần): record TradeInQuote(decimal TradeInValue, Subsidy, Total); Estimate(baseValue, conditionFactor)
    → TradeInValue=round(base×factor), Subsidy=hằng, Total=TradeInValue+Subsidy.
  - TradeInCatalog (tĩnh): Devices[(key,name,base)], Conditions[(key,label,factor)], Subsidy=500.000. FindDevice/FindCondition.
  - TradeInService: Quote(deviceKey,conditionKey)→TradeInQuote? (null nếu key lạ); SubmitAsync(customerId?, deviceKey, conditionKey)
    → validate ∈ catalog, tính lại server-side, tạo TradeInRequest(Model=tên máy, Condition=nhãn, EstimatedPrice=Total, Pending).
- Web: TradeInController GET/POST /thu-cu-doi-moi + TradeInVm + Views Index/Submitted (form từ catalog, ước tính live Alpine).
- Docs: api.md (/thu-cu-doi-moi). data-model: TradeInRequest đã có (CustomerId nullable sẵn) — không đổi.

## Next action
- Chờ duyệt Contract → Cổng 3 test TradeInEstimator (red) → Cổng 4.
- [ ] 2. Contract        — TODO
- [ ] 3. Test (red)      — TODO
- [ ] 4. Implement       — TODO
- [ ] 5. Review subagent — TODO
- [ ] 6. Review tay       — TODO
- [ ] 7. Test (green)    — TODO
- [ ] 8. Commit          — TODO

## Dữ liệu / phạm vi
- TradeInRequest (CustomerId?, Model, Condition, EstimatedPrice, Status=TradeInStatus.Pending) + DbSet đã có
  → KHÔNG đổi schema/migration.
- Định giá: TradeInEstimator THUẦN — baseValue (theo dòng máy chọn sẵn) × hệ số tình trạng + trợ giá (bonus).
  Danh sách dòng máy + base = static trong code (không entity mới).
- Server tính lại estimate khi submit (KHÔNG tin giá client gửi).

## Thiết kế (Cổng 0)
- ShopDunk chỉ có card "Thu cũ đổi mới" (trợ giá đến 3 triệu) ở bento trang chủ, KHÔNG có form định giá.
  → đề xuất tự dựng trang /thu-cu-doi-moi theo SPEC §6 UC-04 + tokens.

## Next action
- Chốt Cổng 0 (tự dựng) → Cổng 1 Spec.
