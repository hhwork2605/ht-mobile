# P3-02 Trả góp 0%

- **Phase:** 3
- **Tính năng SPEC:** §4.2 (trả góp 0%), §5 (nút "Mua trả góp 0%" / "Trả góp 0% qua thẻ")
- **Trạng thái:** doing

## Mục tiêu
Trên PDP, khách thấy có thể **trả góp 0%**: dòng gợi ý "chỉ từ X/tháng" + bảng kỳ hạn (6/9/12 tháng)
với số tiền trả mỗi tháng. Thuần hiển thị/tính toán — chưa nộp hồ sơ/duyệt qua công ty tài chính thật.

## Phạm vi (in-scope)
- [ ] Logic thuần `InstallmentCalculator`: giá → phương án theo kỳ hạn [6,9,12] tháng, lãi 0% →
      trả/tháng = giá ÷ kỳ hạn (làm tròn lên đồng), tổng = giá. Hàm `MonthlyFrom` = trả/tháng kỳ hạn dài nhất (rẻ nhất). Có test.
- [ ] PDP khối giá: dòng "Trả góp 0% chỉ từ {MonthlyFrom}/tháng" (ẩn nếu giá ≤ 0).
- [ ] Nút "Trả góp 0%" (đang là nút chết) → mở **bảng kỳ hạn** (Alpine toggle, server-render): mỗi dòng kỳ hạn + trả/tháng.
- [ ] Dùng giá hiệu lực của variant đang chọn (`Model.Price.FinalPrice`).

## Ngoài phạm vi (out-of-scope)
- Tích hợp công ty tài chính / duyệt hồ sơ / trả góp qua thẻ thật → tích hợp ngoài, Phase sau.
- Lưu đơn trả góp, lãi suất khác 0%, trả trước (down payment), phí chuyển đổi.
- Trả góp ở checkout (Phase 2 checkout chỉ COD).

## File sẽ đụng
- `src/HtMobile.Application/Features/Installment/InstallmentCalculator.cs` (mới, thuần).
- `src/HtMobile.Web/Areas/Storefront/Views/Catalog/ProductDetail.cshtml` (dòng gợi ý + bảng kỳ hạn).
- `tests/HtMobile.Application.UnitTests/Installment/InstallmentCalculatorTests.cs`.

## Phụ thuộc
- Giá hiệu lực qua `IPricingService` (đã có ở `ProductDetailDto.Price`). KHÔNG entity/DB/migration.

## Tiêu chí done
- [ ] Build xanh, Dependency Rule OK (calculator thuần ở Application; view gọi trực tiếp như helper hiển thị).
- [ ] PDP hiện "chỉ từ X/tháng" đúng (= giá ÷ 12, làm tròn lên); bấm "Trả góp 0%" hiện bảng 6/9/12 tháng.
- [ ] Giá ≤ 0 → không hiện trả góp.
- [ ] Unit test: trả/tháng từng kỳ hạn (làm tròn lên), MonthlyFrom, giá 0 → rỗng.

## Cách verify
- PDP iPhone → thấy "Trả góp 0% chỉ từ …/tháng"; bấm "Trả góp 0%" → bảng 6/9/12 tháng đúng số.
- `dotnet test` cho InstallmentCalculator.
