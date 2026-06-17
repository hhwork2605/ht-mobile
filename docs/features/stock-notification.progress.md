# Tiến độ: Theo dõi hàng về (StockNotification) — Phase 3
slug: stock-notification
Cập nhật: 2026-06-17 03:05
Cổng hiện tại: 3
Chế độ: TỰ DUYỆT (người dùng yêu cầu auto-approve các cổng DỪNG cho feature này)

## Trạng thái cổng
- [x] 0. Thiết kế/UI      — DONE (tự dựng theo SPEC §4.1/UC-05)
- [x] 1. Spec            — DONE (tự duyệt)
- [x] 2. Contract        — DONE (tự duyệt; không schema/migration)

- [x] 3. Test (red)      — DONE (ContactValidator đỏ)
- [x] 4. Implement       — DONE
- [x] 5. Review subagent — DONE (Đạt, 0 blocker)
- [x] 6. Review tay       — DONE (tự duyệt: M1 chỉ OutOfStock + siết regex SĐT + test)
- [x] 7. Test (green)     — DONE (103 test pass)
- [x] 8. Commit           — IN_PROGRESS

## Next action
- Smoke-test PDP variant hết hàng → form theo dõi. Phase 3: 4/5 (còn cổng thanh toán).

## Contract (Cổng 2) — chốt
- KHÔNG entity/DB/migration (StockNotification + DbSet đã có).
- App Features/StockNotifications: ContactValidator (thuần: IsValid = email hoặc SĐT VN);
  StockNotificationService.SubscribeAsync(variantId, contact) → enum StockNotifyResult
  {Subscribed, AlreadySubscribed, InvalidContact, VariantNotFound, StillInStock}.
  Quy tắc: contact hợp lệ; variant tồn tại; nếu Active → StillInStock (không theo dõi hàng còn);
  dedupe (variant+contact chưa notified → AlreadySubscribed); else tạo.
- Web: StockNotificationController POST /stock-notify (htmx) → partial _StockNotifyDone/_StockNotifyForm.
  PDP: !InStock → form theo dõi thay MUA NGAY.
- Seed: SeedStockDemoAsync — nếu chưa có variant OutOfStock thì set 1 variant định sẵn (slug dien-thoai-iphone-17-256gb).
- Docs: api.md (/stock-notify).
- [ ] 3. Test (red)      — TODO
- [ ] 4. Implement       — TODO
- [ ] 5. Review subagent — TODO
- [ ] 6. Review tay       — TODO
- [ ] 7. Test (green)    — TODO
- [ ] 8. Commit          — TODO

## Dữ liệu / phạm vi
- StockNotification (VariantId, Contact email/SĐT, Notified) + DbSet đã có → KHÔNG đổi schema.
- PDP có Model.InStock (= variant Status Active). Khi hết hàng (OutOfStock/Discontinued) → hiện form theo dõi
  thay nút MUA NGAY. Submit → tạo StockNotification (dedupe theo variant+contact).
- Logic thuần: ContactValidator (email hoặc SĐT VN hợp lệ) — test.
- Seed: cần 1 variant OutOfStock để demo/test (đề xuất).

## Thiết kế (Cổng 0)
- ShopDunk không có UI theo dõi hàng về. SPEC §4.1/UC-05 mô tả "Theo dõi để biết khi có hàng".
  → đề xuất tự dựng form nhỏ ở PDP (htmx) theo tokens.

## Next action
- Chốt Cổng 0 (tự dựng) → Cổng 1 Spec.
