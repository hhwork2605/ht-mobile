# Tiến độ: Trả góp 0% — Phase 3
slug: installment
Cập nhật: 2026-06-17 02:05
Cổng hiện tại: 1

## Trạng thái cổng
- [x] 0. Thiết kế/UI      — DONE (tự dựng theo SPEC §5 + tokens)
- [x] 1. Spec            — DONE (duyệt)
- [x] 2. Contract        — DONE (duyệt; không schema/route)
- [x] 3. Test (red)      — DONE (5 InstallmentCalculator đỏ)
- [x] 4. Implement       — DONE (build xanh, 78 test pass)
- [x] 5. Review subagent — DONE (Đạt, 0 blocker, chỉ Low)
- [x] 6. Review tay       — DONE (duyệt: +1 test MonthlyFrom)
- [x] 7. Test (green)     — DONE (79 test pass)
- [x] 8. Commit           — IN_PROGRESS

## Next action
- XONG. Phase 3: 2/5 (Bundle, Trả góp). Còn Thu cũ, Theo dõi hàng về, Cổng thanh toán.

## Vấn đề mở (reviewer Cổng 5) — toàn Low
- Bảng kỳ hạn x-cloak ẩn với bot (thông tin phụ, chấp nhận). Format tiền giống _PriceBlock (chấp nhận).
- Nút "Trả góp qua thẻ"/"Thu cũ" vẫn là nút chết (ngoài scope; nằm ngoài form nên vô hại).
- Đề xuất làm: thêm 1 test MonthlyFrom với giá lẻ (đóng test gap). Còn lại chấp nhận.
- Điểm tốt: đúng đắn (Ceiling, 0%, giá≤0 rỗng); thuần/Dependency Rule; XSS an toàn; 1 h1; x-cloak có rule.

## Contract (Cổng 2) — chốt
- KHÔNG entity/DB/migration/route mới. KHÔNG đổi interface.
- App mới: Features/Installment/InstallmentCalculator (thuần):
  record InstallmentPlan(int Months, decimal MonthlyAmount, decimal Total);
  Terms = {6,9,12}; Plans(price) → list (monthly = Ceiling(price/months), total = price); MonthlyFrom(price) = monthly kỳ hạn dài nhất; price ≤ 0 → rỗng.
- Web: ProductDetail.cshtml gọi InstallmentCalculator (helper hiển thị) — dòng "chỉ từ X/tháng" + bảng kỳ hạn (Alpine toggle).
- Docs: không đổi (không có endpoint/entity/route mới).
- [ ] 2. Contract        — TODO
- [ ] 3. Test (red)      — TODO
- [ ] 4. Implement       — TODO
- [ ] 5. Review subagent — TODO
- [ ] 6. Review tay       — TODO
- [ ] 7. Test (green)    — TODO
- [ ] 8. Commit          — TODO

## Thiết kế (Cổng 0)
- ShopDunk PDP có gợi ý "Hoặc trả góp 0% chỉ từ {installText}/tháng" (dòng 268). KHÔNG có bảng kỳ hạn.
- SPEC §5: nút "Mua trả góp 0%" (công ty tài chính) + "Trả góp 0% qua thẻ". → đề xuất tự dựng: 1 dòng "chỉ từ
  X/tháng" + khối/bảng kỳ hạn (6/9/12 tháng, lãi 0%) hiện khi bấm "Trả góp 0%".

## Dữ liệu / phạm vi
- KHÔNG entity mới: trả góp là calculator hiển thị (không lưu, chưa tích hợp công ty tài chính/duyệt hồ sơ thật).
- Logic thuần InstallmentCalculator: giá → các phương án (kỳ hạn) → trả/tháng (0% = giá/kỳ hạn, làm tròn đồng).

## Next action
- Chốt Cổng 0 (tự dựng bảng kỳ hạn theo SPEC) → Cổng 1 Spec.
