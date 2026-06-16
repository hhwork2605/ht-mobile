# Backlog (theo SPEC §12)

> `[ ]` chưa làm · `[~]` đang làm · `[x]` xong. Phase 0 (scaffold) đã xong khi build + 3 trang skeleton chạy.

## Phase 0 — Scaffold (nền tảng)
- [x] Solution Clean Architecture + 8 project + Dependency Rule
- [x] Thư mục vibe-code (CLAUDE.md, docs, tasks, scripts, .claude)
- [~] Domain entities + Enums + Common base
- [~] Application interfaces + feature folders + DI
- [~] Infrastructure: AppDbContext + configs + Identity + Redis + Search + Seed + DI
- [~] Web: Program.cs + Areas + ViewComponents + Tailwind + 3 trang skeleton (Home / `/iphone` / PDP)
- [~] Migration `Initial` + build xanh

## Phase 1 — Catalog & tìm kiếm  → `phase-1-catalog/`
- [x] Trang chủ: banner slider + tile danh mục + lưới SP nổi bật + bento + trust bar (P1-01)
- [ ] Trang danh mục: tab lọc series + lưới SP + block SEO + breadcrumb
- [ ] PDP đầy đủ: gallery, video, VariantSelector, OfferList, cam kết, mô tả, review
- [ ] Search autocomplete (Postgres FTS): keyword + product suggestions
- [x] ProductCard (badge giảm %, mới, giá gạch) + ViewComponent tái dùng (P1-01)

## Phase 2 — Mua hàng  → `phase-2-cart-checkout/`
- [ ] Giỏ hàng (guest theo session + user) + cập nhật SL + empty state
- [ ] Checkout: thông tin nhận hàng + phương thức giao + thanh toán
- [ ] Đăng ký / Đăng nhập / Quên mật khẩu / Nhớ đăng nhập (Identity)
- [ ] Tài khoản + lịch sử đơn hàng
- [ ] Pricing engine đầy đủ: giá theo vùng + khuyến mãi nhiều tầng + cache Redis

## Phase 3 — Nghiệp vụ đặc thù  → `phase-3-business/`
- [ ] Bundle "mua kèm phụ kiện" (tổng tiết kiệm)
- [ ] Trả góp 0% (công ty tài chính + qua thẻ)
- [ ] Thu cũ đổi mới (định giá + trợ giá)
- [ ] Theo dõi hàng về (StockNotification)
- [ ] Tích hợp cổng thanh toán (VNPAY/ZaloPay)

## Phase 4 — Dịch vụ & CMS  → `phase-4-services-cms/`
- [ ] Gói bảo hành / AppleCare (ServicePackage)
- [ ] Bán hàng doanh nghiệp / trường học (báo giá)
- [ ] Blog/newsfeed + trang chính sách (CMS)
- [ ] Đa ngôn ngữ VI/EN (resx + Translation) + /changelanguage
- [ ] Định vị cửa hàng + check IMEI + tra cứu hoá đơn

## Phase 5 — Hoàn thiện  → `phase-5-admin-polish/`
- [ ] Admin: danh mục/SP/biến thể/tồn kho theo cửa hàng & vùng
- [ ] Admin: khuyến mãi/campaign + đơn hàng + người dùng & phân quyền + CMS
- [ ] Báo cáo doanh thu / đơn hàng
- [ ] SEO nâng cao (structured data) + tối ưu hiệu năng + kiểm thử bảo mật
