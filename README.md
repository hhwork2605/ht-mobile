# HtMobile — Apple Reseller E-commerce

Website TMĐT bán lẻ sản phẩm Apple (kiểu ShopDunk) xây bằng **ASP.NET Core MVC (.NET 9)** theo
**Clean Architecture**. Đặc tả: [docs/SPEC.md](docs/SPEC.md).

## Stack

.NET 9 MVC · EF Core 9 + PostgreSQL (Supabase) · Redis · Tailwind CSS v4 + htmx + Alpine.js ·
ASP.NET Core Identity · Postgres Full-Text Search.

## Cấu trúc

```
src/   HtMobile.Domain · Application · Infrastructure · Web   (Clean Architecture, phụ thuộc hướng vào trong)
tests/ Domain.UnitTests · Application.UnitTests · Infrastructure.IntegrationTests · Web.FunctionalTests
docs/  SPEC, architecture, conventions, data-model, api, setup
tasks/ backlog chia theo phase (1 file = 1 đầu việc vibe-code)
scripts/ setup · dev · migrate · seed · db-reset (.ps1 + .sh)
```

Chi tiết kiến trúc: [docs/architecture.md](docs/architecture.md) · Quy ước: [docs/conventions.md](docs/conventions.md).

## Bắt đầu

```powershell
scripts\setup.ps1            # restore + npm install + redis (docker)
# đặt connection Supabase qua user-secrets → docs/setup.md
scripts\migrate.ps1 update   # áp schema
scripts\dev.ps1              # chạy Web + tailwind watch
```

Xem [docs/setup.md](docs/setup.md) cho connection string Supabase (direct vs pooler) và user-secrets.

## Lệnh thường dùng

```powershell
dotnet build HtMobile.sln
dotnet test
scripts\migrate.ps1 add <Name>     # tạo migration mới
```

## Vibe-code

Mở [tasks/backlog.md](tasks/backlog.md), chọn 1 task, theo luồng trong [CLAUDE.md](CLAUDE.md)
("Cách thêm 1 feature"). Có sẵn skill `/new-feature` (quy trình 8 cổng), command `/add-entity` và subagent `code-reviewer`.

## Feature Workflow — biến quy trình 8 cổng thành 1 lệnh

Sau khi cài, bạn chỉ cần gõ:

/new-feature tạo order cho POS: input là branchId, items[], paymentMethod ...

Claude Code sẽ tự chạy spec → contract → test → implement → review → test →
commit, và dừng ở các cổng (spec, contract, review tay) chờ bạn duyệt.

Cài đặt

Cách khuyến nghị (skill — định dạng mới 2026):
Copy thư mục .claude/skills/new-feature/ vào repo của bạn (hoặc vào
~/.claude/skills/ để dùng cho mọi dự án). Claude Code tự nhận. Gọi bằng
/new-feature <mô tả>, hoặc chỉ cần nói "làm tính năng X theo quy trình" là
nó tự kích hoạt.

Copy luôn .claude/agents/code-reviewer.md — đây là subagent chỉ-đọc mà Cổng 5
tự gọi để review độc lập. Không có file này thì Cổng 5 không chạy được.

Cách cũ (slash command — vẫn chạy):
Nếu muốn 1 file đơn giản, đặt nội dung phần thân (bỏ frontmatter name) vào
.claude/commands/new-feature.md. Tên file thành tên lệnh: /new-feature.

Tuỳ biến cho repo của bạn

File quy ước: skill này đọc CLAUDE.md/AGENTS.md của repo để biết
stack, convention, lệnh build/test. Hãy chắc chắn repo có file đó (xem mẫu
AGENTS.md đã bàn trước đây cho KiotViet). Đây mới là nơi quyết định code ra
trông thế nào — skill chỉ điều phối quy trình.
Lệnh build/test: sửa dòng allowed-tools trong SKILL.md cho khớp stack.
Ví dụ hiện đang để dotnet build / dotnet test. Đổi nếu dùng khác.
Cổng dừng: mặc định dừng ở Spec, Contract, và Review tay. Muốn thêm/bớt
cổng dừng thì sửa chữ "DỪNG" trong các mục tương ứng.

Vì sao KHÔNG để chạy hết không cần đụng vào

Các cổng dừng là cố ý. Lý do cả workflow tồn tại là để bạn duyệt spec sớm
(rẻ) thay vì sửa code sai (đắt), và để mắt vào phần tiền/kho/quyền. Một runner
"nhập tính năng rồi đi pha cà phê" sẽ phá đúng giá trị đó. Đây là bán tự động
có chủ đích: máy lo phần lặp đi lặp lại, bạn giữ các quyết định quan trọng.

Kiến trúc agent

Skill /new-feature (parent) ← điều phối 8 cổng, giữ context, duyệt + ghi + commit
└─ Cổng 5 → subagent code-reviewer (read-only, context sạch, chỉ báo cáo)

Chỉ tách đúng một việc ra subagent: review. Hai lý do — (1) agent vừa viết code
thì thiên vị, reviewer context sạch soi gắt hơn; (2) việc đọc cả codebase +
chạy test sinh output dài, đẩy vào subagent để context chính khỏi phình.
Reviewer chỉ-đọc và chỉ báo cáo; mọi việc SỬA/COMMIT vẫn ở parent (nơi xử lý
được prompt duyệt — subagent không hiện được prompt xin phép). Spec, contract,
implement, test, commit đều ở parent vì chúng phụ thuộc nhau và cần bạn duyệt.

Bộ nhớ tiến độ (chống mất mạng / dừng giữa chừng)

Hai lớp, ưu tiên lớp 1:

Lớp 1 — File trạng thái (nguồn sự thật). Skill tự tạo và cập nhật
docs/features/<slug>.progress.md sau MỖI cổng, ghi trước mỗi điểm dừng. Lần
sau mở lại, skill đọc file này, tóm tắt "đang ở Cổng N", và tiếp tục đúng chỗ
— không làm lại cổng đã DONE. File nằm trên đĩa + commit git nên sống sót qua
mọi sự cố, kể cả đổi máy. Đây là lớp đáng tin.

Lớp 2 — Session resume của Claude Code (tiện, không bắt buộc).
claude --continue (hoặc -c) mở lại session gần nhất trong thư mục;
claude --resume mở picker chọn session; đặt tên session theo tính năng
(claude -n "feature-order") rồi claude --resume "feature-order" để nhảy
thẳng vào. Lớp này khôi phục hội thoại, hữu ích để có lại ngữ cảnh thảo luận.

Vì sao không chỉ dựa lớp 2: session chỉ lưu hội thoại chứ không lưu trạng thái
file, hội thoại dài bị nén làm mất chi tiết, và mất mạng đột ngột có thể chưa
lưu sạch. Nên lớp 1 mới là checkpoint chuẩn; lớp 2 là tiện ích bồi thêm.

Dùng với agent khác (Cursor, Gemini CLI...)

Nội dung thân SKILL.md là tool-agnostic. Bạn có thể dán nguyên vào Cursor
(.cursor/rules/) hoặc paste trực tiếp làm prompt mở đầu cho bất kỳ agent nào,
chỉ cần thay $ARGUMENTS bằng mô tả tính năng.
