# P3-03 Thu cũ đổi mới

- **Phase:** 3
- **Tính năng SPEC:** §4.2 (thu cũ đổi mới), §6 UC-04, §7 (TradeInRequest)
- **Trạng thái:** doing

## Mục tiêu
Khách khai **dòng máy cũ + tình trạng** → xem **ước tính giá thu + trợ giá** → gửi yêu cầu; hệ thống lưu
`TradeInRequest` (Pending) để nhân viên liên hệ. Thuần định giá ước tính, chưa thẩm định máy thật.

## Phạm vi (in-scope)
- [ ] Trang `/thu-cu-doi-moi` (GET): form chọn **dòng máy** (danh sách định sẵn) + **tình trạng** (Tốt/Khá/Trung bình)
      + ước tính **live** (Alpine, từ giá base + hệ số nhúng) → hiện giá thu + trợ giá + tổng nhận.
- [ ] Logic thuần `TradeInEstimator`: giá thu = round(base × hệ số tình trạng); trợ giá = bonus cố định; tổng = giá thu + trợ giá. Có test.
- [ ] `TradeInCatalog` (tĩnh, không entity): danh sách dòng máy (key + tên + base) + tình trạng (key + nhãn + hệ số) + trợ giá.
- [ ] `POST /thu-cu-doi-moi` (antiforgery): validate device/condition ∈ catalog, **tính lại estimate server-side** (không tin client),
      tạo `TradeInRequest` (CustomerId nếu đăng nhập, else null; Model, Condition, EstimatedPrice=tổng, Status=Pending) → màn xác nhận.

## Ngoài phạm vi (out-of-scope)
- Thẩm định máy thật / chốt giá cuối / áp trợ giá trực tiếp vào đơn mua mới (chỉ ước tính + ghi nhận yêu cầu).
- Quản lý yêu cầu thu cũ ở Admin (Phase 5). Upload ảnh máy. Danh mục dòng máy động (Phase sau).

## File sẽ đụng
- `src/HtMobile.Application/Features/TradeIn/TradeInEstimator.cs` (thuần) + `TradeInCatalog.cs` + Dtos.
- `src/HtMobile.Application/Features/TradeIn/TradeInService.cs` (validate + tạo request).
- `src/HtMobile.Web/Areas/Storefront/Controllers/TradeInController.cs` + Views (Index form + Submitted).
- `src/HtMobile.Web/Models/TradeIn/TradeInVm.cs`.
- `tests/HtMobile.Application.UnitTests/TradeIn/TradeInEstimatorTests.cs`.

## Phụ thuộc
- `TradeInRequest` + DbSet (đã có), `ICurrentUser` (P2-03). KHÔNG entity/migration mới.

## Tiêu chí done
- [ ] Build xanh, Dependency Rule OK (DB qua IApplicationDbContext; estimator thuần).
- [ ] Form hiện ước tính đúng theo dòng máy + tình trạng; gửi → lưu TradeInRequest (Pending) + màn xác nhận có số ước tính.
- [ ] Device/condition lạ (không ∈ catalog) → từ chối (không tạo request giá bậy).
- [ ] Guest gửi được (CustomerId null); user đăng nhập → CustomerId set.
- [ ] Unit test: estimator (giá thu round, trợ giá, tổng theo từng tình trạng) + catalog lookup key lạ → null.

## Cách verify
- /thu-cu-doi-moi → chọn iPhone 13 + Tốt → thấy ước tính → Gửi → màn xác nhận; DB có TradeInRequest Pending.
- `dotnet test` cho TradeInEstimator.
