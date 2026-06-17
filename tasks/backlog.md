# Backlog (theo SPEC §12)

> `[ ]` chưa làm · `[~]` đang làm · `[x]` xong. Phase 0 (scaffold) đã xong khi build + 3 trang skeleton chạy.

## Phase 0 — Scaffold (nền tảng)
- [x] Solution Clean Architecture + 8 project + Dependency Rule
- [x] Thư mục vibe-code (CLAUDE.md, docs, tasks, scripts, .claude)
- [x] Domain entities + Enums + Common base
- [x] Application interfaces + feature folders + DI
- [x] Infrastructure: AppDbContext + configs + Identity + Redis + Search + Seed + DI
- [x] Web: Program.cs + Areas + ViewComponents + Tailwind + 3 trang skeleton (Home / `/iphone` / PDP)
- [x] Migration `Initial` + build xanh

## Phase 1 — Catalog & tìm kiếm  → `phase-1-catalog/`
- [x] Trang chủ: banner slider + tile danh mục + lưới SP nổi bật + bento + trust bar (P1-01)
- [x] Trang danh mục: tab lọc series (client-side) + lưới SP + block SEO + breadcrumb + JSON-LD ItemList/BreadcrumbList
- [x] PDP đầy đủ: gallery+thumbnail, video, VariantSelector, OfferList + ưu đãi thanh toán, cam kết, mô tả, review summary + JSON-LD Product
- [x] Search: trang kết quả /search (server-render, noindex) + autocomplete partial + sitemap.xml/robots.txt
- [x] ProductCard (badge giảm %, mới, giá gạch) + ViewComponent tái dùng (P1-01)
- [x] Nền SEO: _Layout (canonical/OG/hreflang/JSON-LD Organization+WebSite) + helper JsonLd

## Phase 2 — Mua hàng  → `phase-2-cart-checkout/`
- [x] Giỏ hàng (guest theo session + user) + cập nhật SL + empty state (P2-01) — *migration CartUniqueIndexes chờ apply*
- [x] Checkout: thông tin nhận hàng + phương thức giao + thanh toán (P2-02, COD/guest) — *migration OrderCustomerNullable chờ apply*
- [x] Đăng ký / Đăng nhập / Quên mật khẩu / Nhớ đăng nhập (Identity) (P2-03) + merge giỏ guest→user
- [x] Tài khoản + lịch sử đơn hàng (P2-04) — dashboard Đơn hàng + Thông tin (Wishlist dời feature riêng)
- [x] Pricing engine đầy đủ: khuyến mãi nhiều tầng (best%+best fixed) + điều kiện ConditionsJson + cache (P2-05, giá một mức — ADR-0002)

## Phase 3 — Nghiệp vụ đặc thù  → `phase-3-business/`
- [x] Bundle "mua kèm phụ kiện" (tổng tiết kiệm) (P3-01) — *migration CartItemUnitPriceOverride chờ apply*
- [x] Trả góp 0% (P3-02) — calculator + bảng kỳ hạn trên PDP (chưa tích hợp công ty tài chính/thẻ thật)
- [x] Thu cũ đổi mới (định giá + trợ giá) (P3-03) — form /thu-cu-doi-moi, estimate server-side, tạo TradeInRequest
- [x] Theo dõi hàng về (StockNotification) (P3-04) — form PDP khi hết hàng, dedupe, chỉ OutOfStock
- [ ] Tích hợp cổng thanh toán (VNPAY/ZaloPay)

## Phase 4 — Dịch vụ & CMS  → `phase-4-services-cms/`
- [ ] Gói bảo hành / AppleCare (ServicePackage)
- [ ] Bán hàng doanh nghiệp / trường học (báo giá)
- [ ] Blog/newsfeed + trang chính sách (CMS)
- [ ] Đa ngôn ngữ VI/EN (resx + Translation) + /changelanguage
- [ ] Định vị cửa hàng + check IMEI + tra cứu hoá đơn

## Phase 5 — Hoàn thiện  → `phase-5-admin-polish/`
- [ ] Admin: danh mục/SP/biến thể/tồn kho theo cửa hàng
- [ ] Admin: khuyến mãi/campaign + đơn hàng + người dùng & phân quyền + CMS
- [ ] Báo cáo doanh thu / đơn hàng
- [ ] SEO nâng cao (structured data) + tối ưu hiệu năng + kiểm thử bảo mật
