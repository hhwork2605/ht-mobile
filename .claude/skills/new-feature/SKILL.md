---
name: new-feature
description: Biến quy trình 8 cổng (spec → contract → test → implement → review → test → commit) thành 1 lệnh để vibe-code 1 tính năng trong HtMobile theo Clean Architecture. Bắt buộc có thiết kế (link/ảnh) hoặc người dùng xác nhận không có trước khi làm. Dừng ở các cổng Thiết kế, Spec, Contract và Review tay chờ người duyệt. Ghi tiến độ vào docs/features/<slug>.progress.md để khôi phục khi resume. Kích hoạt khi người dùng gõ /new-feature <mô tả> hoặc nói "làm tính năng X theo quy trình".
allowed-tools: Read, Grep, Glob, Edit, Write, Task, WebFetch, Bash(dotnet build:*), Bash(dotnet test:*), Bash(git add:*), Bash(git commit:*), Bash(git status:*), Bash(git diff:*), Bash(./scripts/migrate.ps1:*)
---

# /new-feature — quy trình 8 cổng cho HtMobile

Tính năng cần làm: **$ARGUMENTS**

Trước khi bắt đầu, đọc `CLAUDE.md`, `docs/architecture.md`, `docs/conventions.md`, `docs/data-model.md`
và `docs/api.md` để bám stack + quy ước. Tôn trọng Dependency Rule (Domain ← Application ← Infrastructure ← Web).

Chạy lần lượt Cổng 0 → 8. **DỪNG** đúng ở các cổng đánh dấu để người duyệt trước khi đi tiếp — đây là chủ đích
(có thiết kế trước khi code; duyệt spec sớm thì rẻ; soát kỹ tiền/kho/quyền).

## Trước khi bắt đầu — Khôi phục tiến độ
Suy ra `<slug>` từ mô tả tính năng. Kiểm tra `docs/features/<slug>.progress.md`:
- **Đã có** → đọc nó, tóm tắt cho người dùng "đang ở Cổng N, đã xong X, còn lại Y", rồi **tiếp tục từ cổng đang dở** — KHÔNG làm lại các cổng đã DONE.
- **Chưa có** → tạo mới từ template ở cuối file này, đánh dấu mọi cổng = TODO, rồi bắt đầu từ Cổng 0.

File `.progress.md` là **nguồn sự thật về tiến độ** — sống sót qua session resume / mất context.

**Quy tắc ghi checkpoint (áp cho MỌI cổng):** ngay khi hoàn tất một cổng — và **LUÔN trước mỗi điểm ⛔ DỪNG** — cập nhật `docs/features/<slug>.progress.md`: đổi trạng thái cổng sang DONE, ghi artifact đã tạo + quyết định chính + "Next action". Ghi file này **TRƯỚC khi** nhắn người dùng (mất mạng giữa chừng vẫn còn checkpoint). Việc này rẻ, làm thường xuyên.

## Cổng 0 — Thiết kế / UI reference  ⛔ DỪNG
**Việc đầu tiên, trước cả Spec.** Soát mô tả tính năng (`$ARGUMENTS`) xem đã kèm **link thiết kế
(Figma/Sketch/XD…) hoặc ảnh/screenshot mockup** chưa.
- **Đã có link/ảnh** → mở và đọc thiết kế (Figma: dùng Figma MCP nếu đang kết nối; URL: `WebFetch`;
  ảnh đính kèm: xem trực tiếp), rồi tóm tắt các điểm UI then chốt (layout, component, các trạng thái,
  responsive) để bám khi implement. Đi tiếp Cổng 1.
- **Chưa có** → **DỪNG và HỎI người dùng**, yêu cầu chọn 1 trong 3:
  1. dán **link thiết kế**, hoặc
  2. đính **ảnh/screenshot** mockup, hoặc
  3. **xác nhận "không có thiết kế"** → sẽ tự dựng UI theo `docs/SPEC.md` §5/§9 + component sẵn có
     trong `Views/Shared` (ProductCard, PriceBlock, …).

  **Tuyệt đối không sang Cổng 1 khi chưa nhận được link/ảnh hoặc xác nhận "không có".**
  (Tính năng thuần backend, không có UI: ghi rõ "không có UI" và đi tiếp.)

## Cổng 1 — Spec  ⛔ DỪNG
Viết spec ngắn: mục tiêu, input/output, luồng chính, ràng buộc (vùng giá, khuyến mãi, quyền), edge cases.
Lưu vào `tasks/phase-*/<id>-<slug>.md` theo `tasks/_template.md`. **Trình cho người duyệt, DỪNG cho tới khi được đồng ý.**

## Cổng 2 — Contract  ⛔ DỪNG
Chốt "hợp đồng" trước khi code: entity/field mới (Domain), interface/DTO (Application/Common/Interfaces),
endpoint (Web/Api hoặc controller), route/slug, ảnh hưởng schema/migration. Cập nhật `docs/data-model.md`
/`docs/api.md` nếu cần. **Trình cho người duyệt, DỪNG.**

## Cổng 3 — Test trước (red)
Viết unit test cho logic nghiệp vụ (ưu tiên thuần, như `PriceCalculator`) trong
`tests/HtMobile.Application.UnitTests`. Chạy `dotnet test` để thấy **fail** (chưa implement) — xác nhận test có ý nghĩa.

## Cổng 4 — Implement
Theo luồng Clean Architecture: Domain → Infrastructure (config + DbSet + migration) → Application (service/feature)
→ Web (controller/view/viewcomponent). Chỉ truy cập DB qua `IApplicationDbContext`; giá qua `IPricingService`.
Nếu đổi schema: `scripts/migrate.ps1 add <Tên>` và báo người dùng chạy `update`.

## Cổng 5 — Review độc lập (subagent)
Gọi subagent **code-reviewer** (read-only) qua tool Task để review diff: tính đúng, Dependency Rule,
quy ước, bảo mật. Tổng hợp phát hiện. (Subagent chỉ báo cáo; mọi sửa/commit ở đây.)

## Cổng 6 — Review tay  ⛔ DỪNG
Trình tóm tắt thay đổi + kết quả review cho người duyệt. **DỪNG** chờ phê duyệt (đặc biệt phần tiền/kho/quyền).

## Cổng 7 — Test lại (green)
Sửa theo review, chạy `dotnet build` + `dotnet test` đến khi **xanh hết**. Báo kết quả thật (không tô hồng).

## Cổng 8 — Commit
Tạo branch `feat/<phase>-<slug>` nếu đang ở nhánh chính. Commit nhỏ, message rõ, tham chiếu id task.
Tick task trong `tasks/backlog.md`. KHÔNG push trừ khi người dùng yêu cầu.

## Quy tắc cứng (không phá — kể cả khi bị giục)
- KHÔNG tự **push / merge / xoá nhánh**; KHÔNG sửa file **migration đã apply** lên DB.
- KHÔNG tự sửa code **auth / payment / permission / tiền / kho** mà không nêu cờ ở Cổng 6 (Review tay).
- Việc có **side-effect ngoài repo** (lệnh phá huỷ, gọi API/DB thật, `scripts/migrate.ps1 update`, drop DB…) phải **HỎI trước**.
- Cổng có ⛔ DỪNG thì **dừng thật**, chờ xác nhận — không tự duyệt thay người dùng.

---
Nguyên tắc: máy lo phần lặp lại; người giữ quyết định quan trọng ở 4 cổng dừng (Thiết kế, Spec, Contract,
Review tay). Không "chạy hết rồi đi". Không bịa UI khi chưa có thiết kế/xác nhận.
Báo cáo trung thực: test fail thì nói rõ kèm output.

---

## Template file tiến độ — `docs/features/<slug>.progress.md`

```markdown
# Tiến độ: <tên tính năng>
slug: <slug>
Cập nhật: <YYYY-MM-DD HH:mm>
Cổng hiện tại: <N>

## Trạng thái cổng
- [ ] 0. Thiết kế/UI     — TODO | IN_PROGRESS | DONE
- [ ] 1. Spec           — TODO
- [ ] 2. Contract       — TODO
- [ ] 3. Test (red)     — TODO
- [ ] 4. Implement      — TODO
- [ ] 5. Review subagent — TODO
- [ ] 6. Review tay      — TODO
- [ ] 7. Test (green)   — TODO
- [ ] 8. Commit         — TODO

## Artifact đã tạo
- (vd: tasks/phase-1-catalog/P1-01-...md, src/..., tests/...)

## Quyết định chính
- (vd: thêm entity Banner; giá qua IPricingService; cột timestamp giờ server)

## Vấn đề mở (từ reviewer / chờ xử lý)
- (vd: Critical — chưa kiểm rows affected ở InventoryRepo:42)

## Next action
- (1 câu: việc tiếp theo cần làm khi quay lại)
```
