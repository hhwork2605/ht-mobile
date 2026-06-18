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
- [x] Đa ngôn ngữ VI/EN (resx + IStringLocalizer) + /changelanguage (P3-05) — switcher VI|EN, cookie culture; dịch nội dung DB (Translation) hoãn
- [ ] Tích hợp cổng thanh toán (VNPAY/ZaloPay) — ⏸️ **HOÃN, LÀM SAU** (2026-06-17): cần credentials/sandbox thật + webhook IPN, là tích hợp ngoài. Hiện checkout chỉ COD (P2-02). Khi làm: thêm IPaymentGateway (Application) + adapter ở Infrastructure, không phá luồng đặt hàng hiện có.

## Phase 4 — ❌ ĐÃ BỎ (2026-06-17)
> Theo quyết định: chuyển **Đa ngôn ngữ** lên Phase 3, **bỏ** các tính năng còn lại (Gói bảo hành/AppleCare,
> Bán hàng doanh nghiệp/trường học, Blog/newsfeed + trang chính sách CMS, Định vị cửa hàng + check IMEI + tra cứu hoá đơn).
> Thư mục `phase-4-services-cms/` để trống. Nếu cần lại sau này thì mở lại từ SPEC §4.

## Phase 5 — Hoàn thiện  → `phase-5-admin-polish/`
- [ ] Admin: danh mục/SP/biến thể/tồn kho theo cửa hàng
- [~] Admin: khuyến mãi/campaign + đơn hàng + người dùng & phân quyền + CMS
  - [x] Đơn hàng (P5-01) — layout admin + danh sách/lọc + chi tiết + đổi trạng thái (validate luồng)
- [ ] Báo cáo doanh thu / đơn hàng
- [ ] SEO nâng cao (structured data) + tối ưu hiệu năng + kiểm thử bảo mật

## Phase 6 — Storefront UX & hoàn thiện bán hàng  → `phase-6-storefront-ux/`
> Backlog từ đợt **rà soát như người dùng thật** (2026-06-18) trên storefront đã redesign. Ưu tiên P0 → P2.
> `(✓)` = đã kiểm chứng trực tiếp trong phiên rà soát.

### P0 — ảnh hưởng trực tiếp chuyển đổi / khó dùng ngay
- [ ] **Search tiếng Việt theo danh mục + từ đồng nghĩa** (✓) — "tai nghe" → 0 kết quả dù có AirPods; hiện chỉ khớp tiền tố tên. Cần khớp tên danh mục + bộ synonym (tai nghe→AirPods, điện thoại→iPhone, laptop→Mac…) + nhiều từ khoá. (`ISearchService`/Postgres FTS)
- [x] **Luồng "Mua ngay" → thẳng thanh toán** (✓, 2026-06-18) — `CartController.Add` nhận cờ `buyNow` → add rồi redirect `/checkout`; PDP "Mua ngay" gắn `buyNow=true`, "Thêm vào giỏ" giữ về `/cart`.
- [x] **Mã giảm giá ở giỏ/checkout** (✓, 2026-06-18) — `Promotion.Code` (voucher, unique) + `CouponCalculator` (validate hạn/đơn tối thiểu, %/fixed, chặn ≤ subtotal); `Cart.CouponCode` + apply/remove (htmx); Order lưu `Subtotal`/`DiscountAmount`/`CouponCode`; PricingEngine loại KM có Code khỏi auto-apply; admin Khuyến mãi + đơn hàng hiện mã/giảm; seed `GIAM5`/`GIAM500K`. (+8 unit test)
- [x] **Tra cứu đơn cho khách vãng lai (COD)** (✓, 2026-06-18) — trang `/tra-cuu-don` (mã đơn + SĐT); `OrderHistoryService.LookupAsync` verify SĐT khớp Shipment.Address (không lộ đơn nếu sai); tái dùng partial `_OrderCard` (Account cũng dùng chung). Footer "Tra cứu đơn hàng" → /tra-cuu-don.

### P1 — thiếu so với kỳ vọng & so với design
- [x] **PDP: "Sản phẩm liên quan"** (✓, 2026-06-18) — `ProductDetailDto.Related` (4 model cùng danh mục, trừ chính nó; tái dùng `BuildCardsAsync`); khối lưới 4 cột `vc:product-card` cuối PDP.
- [x] **Đánh giá sản phẩm: đọc + viết** (✓, 2026-06-18) — PDP: tổng điểm + thanh phân bố sao + danh sách review (join tên Customer); form "Viết đánh giá" (yêu cầu đăng nhập, star picker) qua `ReviewService` (1 review/khách/model, gửi lại = cập nhật); seed review demo.
- [x] **Trang nội dung + footer hết link chết** (✓, 2026-06-18) — render `SlugKind.Page` qua `/{slug}` → view `ContentPage` (SEO + breadcrumb); seed 6 trang CMS (bảo hành, trả góp 0%, hệ thống cửa hàng, tuyển dụng, tin tức, liên hệ); footer 8 mục giờ là `<a>` thật (Thu cũ đổi mới→/thu-cu-doi-moi, Tra cứu đơn→/account). *Quản trị trang CMS để sau (Phase 5).*
- [x] **Chọn biến thể trực quan** (✓, 2026-06-18) — `VariantAxisBuilder` (thuần, +5 test) tách mỗi thuộc tính thành 1 trục; PDP render bộ chọn riêng **Dung lượng** (nút) + **Màu** (chip có chấm màu), đổi 1 trục giữ nguyên trục kia (fallback nếu tổ hợp không có). Bỏ nút gộp. **+ Đổi giá tức thì client-side** (nhúng `VariantPrices` JSON + Alpine): chọn biến thể cập nhật giá/khuyến mãi/trả góp/nút mua + URL (`replaceState`) **không reload**; hết hàng → form theo dõi (variantId động).
- [x] **Menu danh mục cho mobile** (✓, 2026-06-18) — thêm hamburger (ik-bars) + **drawer trượt** (Alpine, overlay, ESC/click đóng) chứa đầy đủ danh mục (`vc:menu`) + link nhanh (tra cứu đơn, thu cũ, trả góp, tài khoản).

### P2 — hoàn thiện trải nghiệm
- [ ] **Upload ảnh sản phẩm thật** (✓) — hiện toàn placeholder.
- [ ] **Cổng thanh toán thật** — ngoài COD: chuyển khoản/thẻ/trả góp (trùng mục HOÃN ở Phase 3 — gộp khi làm).
- [ ] **Phí & thời gian giao hàng theo địa chỉ** — hiện cứng "Miễn phí"; thêm chọn tỉnh/quận + tính phí/ETA.
- [~] **Tài khoản đầy đủ** — ✅ (2026-06-18) **Sổ địa chỉ** (CRUD + mặc định, `AddressService`; tự điền checkout) + **Đổi mật khẩu** (`CustomerAccountService.ChangePasswordAsync`). ⬜ Còn: huỷ đơn, mua lại đơn (chưa làm đợt này).
- [ ] **Wishlist (yêu thích)** (✓) + **So sánh sản phẩm**.
- [ ] **Hiển thị tồn kho** — số lượng còn / cảnh báo sắp hết (hiện chỉ "Còn hàng").
- [ ] **Newsletter / đăng ký nhận tin**.

## Phase 7 — UX TMĐT (rà soát chuyên gia UI/UX 2026-06-18)
> Phát hiện khi đánh giá storefront theo chuẩn thương mại điện tử (chuyển đổi + tin cậy). Ưu tiên P0 → P2.
> Một số mục trùng P2 ở trên — ghi chú để tránh làm lặp.

### P0 — ma sát / tin cậy ảnh hưởng chuyển đổi
- [x] **Mini-cart toast khi "Thêm vào giỏ"** (✓, 2026-06-18) — "Thêm vào giỏ" (card + PDP) dùng `hx-post` (không reload); `CartController.Add` trả OOB badge (header + bottom-nav) cho htmx; layout nghe `htmx:afterRequest` (path `/cart/items`) → bắn `cart-added` → Alpine hiện toast "Đã thêm vào giỏ hàng" (tự ẩn 2.6s, có "Xem giỏ"). "Mua ngay" giữ điều hướng /checkout. *(Mini-cart drawer để sau nếu cần.)*
- [ ] **Ảnh thật thay placeholder "HtMobile"** — banner/thẻ/gallery đang là ô xám trông như ảnh vỡ; hero text đè watermark. *(= P2 "Upload ảnh sản phẩm thật" — gộp; tối thiểu dùng skeleton/ảnh trung tính.)*
- [ ] **Tin cậy gần nút Mua (PDP)** — đưa badge bảo hành chính hãng / đổi trả 30 ngày / thời gian giao dự kiến lên cạnh giá–nút mua (hiện trust grid ở xa phía dưới).

### P1 — kỳ vọng chuẩn TMĐT
- [x] **Sticky "Thêm vào giỏ" trên mobile PDP** (✓, 2026-06-18) — thanh mua dính đáy (`bottom-[64px]`, md:hidden, trên bottom-nav) trong scope `pdpBuyBox()`: tên + giá (`current.finalPrice`) + nút Thêm (hx-post, không reload) + Mua ngay; chỉ hiện khi còn hàng; thêm `pb-[96px] md:pb-[50px]` tránh che nội dung.
- [x] **Lọc + sort cho trang Tìm kiếm** (✓, 2026-06-18) — `/search` thêm chip lọc khoảng giá + `<select>` sắp xếp (Liên quan / giá ↑ / giá ↓) client-side (Alpine, `data-price`/`data-idx`, reorder DOM) như trang danh mục.
- [x] **Hero mạnh hơn + slider** (✓, 2026-06-18) — banner chính (desktop + mobile) thành carousel Alpine tự chạy 5s (mũi tên + chấm, hover dừng); slide 0 server-render sẵn (SEO/no-JS, giữ đúng 1 `<h1>`/khối), slide sau `display:none` tới khi Alpine chạy; CTA pill nổi + chevron.
- [x] **Bước tiến trình + ghi chú ở Checkout** (✓, 2026-06-18) — thanh 3 bước (Giỏ hàng ✓ → Thông tin & thanh toán → Hoàn tất) + ô "Ghi chú đơn hàng" (`CheckoutVm.Note`, ≤500 ký tự); note đính vào địa chỉ giao khi đặt (`· Ghi chú: …`, chưa thêm cột → tránh migration).
- [x] **Freeship threshold ở giỏ** (✓, 2026-06-18) — `_CartBody` thêm thanh tiến trình freeship (ngưỡng 500.000₫): đạt → "Đơn được miễn phí vận chuyển!", chưa đạt → "Mua thêm X để freeship" + progress bar. *(Cross-sell "Có thể bạn thích" để sau.)*
- [x] **Icon phương thức thanh toán + cam kết bảo mật** (✓, 2026-06-18) — partial `_PaymentMethods` (VISA/Mastercard/JCB badge + ATM/COD icon + "Thanh toán an toàn & bảo mật") dùng chung ở footer + mục thanh toán checkout.

### P2 — nâng cao
- [x] **Đã xem gần đây** (✓, 2026-06-18) — partial `_RecentlyViewed` (Alpine, localStorage `htm_recent`): PDP tự đẩy SP vừa xem (`window.htmRecentPush`), rail hiện ở trang chủ + cuối PDP (loại chính nó); ẩn hoàn toàn khi trống (không gây "ồn" cho khách mới); có nút Xoá. *(Gợi ý cá nhân hoá server-side để sau.)*
- [x] **Bỏ/đổi dữ liệu trang trí gây hiểu nhầm** (✓, 2026-06-18) — Flash Sale: bỏ thanh "Đang bán chạy" giả (random theo %) → thay bằng "Tiết kiệm X₫" (thật); đếm ngược đổi từ vòng lặp tự reset → đếm tới **hết ngày** (mốc thật). *(Rating tĩnh trên thẻ: ProductCard hiện không hiển thị rating nên không còn vấn đề.)*
- [x] **Gọn trang chủ** (✓, 2026-06-18) — "Sản phẩm nổi bật" loại các SP đã xuất hiện ở Flash Sale (trước đây trùng hoàn toàn) → hết lặp lưới; "Đã xem gần đây" chỉ hiện khi có dữ liệu.
> Trùng P2 đã có: Wishlist + So sánh (#14), Hiển thị tồn kho (#15), Newsletter (#16), Cổng thanh toán (#11), Phí/ETA giao hàng (#12).
