# tasks/ — backlog vibe-code

Mỗi file trong các thư mục `phase-*` là **1 đầu việc cho 1 phiên vibe-code**: đủ nhỏ để hoàn thành +
test + commit trong một lần. Bám lộ trình [docs/SPEC.md](../SPEC.md) §12.

## Quy trình
1. Mở [backlog.md](backlog.md), chọn 1 task chưa làm.
2. Mở file task tương ứng (tạo từ [`_template.md`](_template.md) nếu chưa có).
3. Vibe-code theo "Phạm vi" + "File sẽ đụng"; bám [docs/conventions.md](../docs/conventions.md).
4. Tự kiểm theo "Tiêu chí done" + "Cách verify".
5. Tick task trong `backlog.md`, commit kèm id task.

## Mẹo cho phiên hiệu quả
- 1 task = 1 vertical slice (Domain → Application → Infrastructure → Web) nếu có thể.
- Nếu task phình to → tách nhỏ trước khi code.
- Luôn nhắc Claude đọc `CLAUDE.md` + file task; không tự đoán chỗ đặt file.
