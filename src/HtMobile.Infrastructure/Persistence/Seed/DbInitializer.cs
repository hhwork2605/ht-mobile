using HtMobile.Application.Common.Interfaces;
using HtMobile.Domain.Constants;
using HtMobile.Domain.Entities.Catalog;
using HtMobile.Domain.Entities.Cms;
using HtMobile.Domain.Entities.Customers;
using HtMobile.Domain.Entities.Pricing;
using HtMobile.Domain.Entities.Sales;
using HtMobile.Domain.Enums;
using HtMobile.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AttributeEntity = HtMobile.Domain.Entities.Catalog.Attribute;

namespace HtMobile.Infrastructure.Persistence.Seed;

/// <summary>Apply migration + seed dữ liệu mẫu. Gọi lúc khởi động ở Development.</summary>
public class DbInitializer
{
    public const string AdminEmail = "admin@htmobile.local";
    public const string AdminPassword = "Admin@123456";

    public const string CustomerEmail = "customer@htmobile.local";
    public const string CustomerPassword = "Customer@123";

    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole<long>> _roles;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(
        AppDbContext db,
        UserManager<ApplicationUser> users,
        RoleManager<IdentityRole<long>> roles,
        IPasswordHasher passwordHasher,
        ILogger<DbInitializer> logger)
    {
        _db = db;
        _users = users;
        _roles = roles;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public static async Task RunAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var init = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        await init.InitialiseAsync();
        await init.SeedAsync();
    }

    public async Task InitialiseAsync()
    {
        _logger.LogInformation("Applying migrations...");
        await _db.Database.MigrateAsync();
    }

    public async Task SeedAsync()
    {
        await SeedRolesAndAdminAsync();
        await SeedDemoCustomerAsync();
        await SeedCategoriesAsync();
        await SeedSampleCatalogAsync();
        await BackfillProductSpecsAsync();
        await SeedVouchersAsync();
        await SeedBannersAsync();
        await SeedBundlesAsync();
        await SeedStockDemoAsync();
        _logger.LogInformation("Seed completed.");
    }

    private async Task SeedRolesAndAdminAsync()
    {
        foreach (var role in Roles.All)
            if (!await _roles.RoleExistsAsync(role))
                await _roles.CreateAsync(new IdentityRole<long>(role));

        // Dọn role Identity "Customer" cũ (ADR 0005 — storefront dùng bảng Customers, không dùng role Identity).
        // Xoá role kéo theo các ánh xạ AspNetUserRoles (cascade) — vốn đã vô nghĩa.
        var legacyCustomerRole = await _roles.FindByNameAsync("Customer");
        if (legacyCustomerRole is not null)
        {
            var result = await _roles.DeleteAsync(legacyCustomerRole);
            if (!result.Succeeded)
                _logger.LogWarning("Bỏ qua xoá role 'Customer' cũ: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        if (await _users.FindByEmailAsync(AdminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                FullName = "Administrator"
            };
            var result = await _users.CreateAsync(admin, AdminPassword);
            if (result.Succeeded)
                await _users.AddToRoleAsync(admin, Roles.Admin);
            else
                _logger.LogWarning("Tạo admin thất bại: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    /// <summary>Voucher demo (có Code → chỉ áp khi khách nhập mã). Idempotent: thêm nếu chưa có (theo Code).</summary>
    private async Task SeedVouchersAsync()
    {
        var now = DateTime.Now;
        if (!await _db.Promotions.AnyAsync(p => p.Code == "GIAM5"))
            _db.Promotions.Add(new Promotion
            {
                Name = "Giảm 5% toàn đơn", Code = "GIAM5", Type = PromotionType.Percentage, Value = 5,
                StartsAt = now.AddDays(-1), EndsAt = now.AddMonths(3)
            });
        if (!await _db.Promotions.AnyAsync(p => p.Code == "GIAM500K"))
            _db.Promotions.Add(new Promotion
            {
                Name = "Giảm 500K cho đơn từ 10 triệu", Code = "GIAM500K", Type = PromotionType.FixedAmount, Value = 500_000,
                ConditionsJson = "{\"minOrder\":10000000}",
                StartsAt = now.AddDays(-1), EndsAt = now.AddMonths(3)
            });
        await _db.SaveChangesAsync();
    }

    /// <summary>Tài khoản Customer demo cho storefront (đăng nhập thử). Tách khỏi Identity (admin) — ADR 0004.</summary>
    private async Task SeedDemoCustomerAsync()
    {
        if (await _db.Customers.AnyAsync(c => c.Email == CustomerEmail)) return;

        _db.Customers.Add(new Customer
        {
            Email = CustomerEmail,
            PasswordHash = _passwordHasher.Hash(CustomerPassword),
            FullName = "Khách Demo",
            Phone = "0900000000",
        });
        await _db.SaveChangesAsync();
    }

    private async Task SeedCategoriesAsync()
    {
        if (await _db.Categories.AnyAsync()) return;

        var roots = new (string Name, string Slug)[]
        {
            ("iPhone", "iphone"), ("iPad", "ipad"), ("Mac", "mac"), ("Apple Watch", "apple-watch"),
            ("Phụ kiện", "phu-kien"), ("Âm thanh", "am-thanh"), ("Camera", "camera"),
            ("Gia dụng", "gia-dung"), ("Máy cũ", "may-cu")
        };

        var order = 0;
        foreach (var (name, slug) in roots)
            _db.Categories.Add(new Category { Name = name, Slug = slug, SortOrder = order++ });

        await _db.SaveChangesAsync();
    }

    private async Task SeedSampleCatalogAsync()
    {
        if (await _db.Products.AnyAsync()) return;

        var now = DateTime.Now;
        var iphone = await _db.Categories.FirstAsync(c => c.Slug == "iphone");
        var ipad = await _db.Categories.FirstAsync(c => c.Slug == "ipad");
        var mac = await _db.Categories.FirstAsync(c => c.Slug == "mac");
        var watch = await _db.Categories.FirstAsync(c => c.Slug == "apple-watch");
        var phukien = await _db.Categories.FirstAsync(c => c.Slug == "phu-kien");

        // Khuyến mãi 10% áp cho mọi sản phẩm (scaffold, auto-apply — Code null)
        _db.Promotions.Add(new Promotion
        {
            Name = "Ưu đãi khai trương -10%",
            Type = PromotionType.Percentage,
            Value = 10,
            StartsAt = now.AddDays(-1),
            EndsAt = now.AddMonths(1)
        });

        // Thuộc tính master dùng chung (EAV — ADR 0003). EF tự gán Id khi SaveChanges.
        _storageAttr = new AttributeEntity { Name = "Dung lượng", SortOrder = 1 };
        _colorAttr = new AttributeEntity { Name = "Màu", SortOrder = 2 };
        _db.Attributes.AddRange(_storageAttr, _colorAttr);

        AddProductWithVariants(
            iphone.Id,
            name: "iPhone 17 Pro Max",
            slug: "dien-thoai-iphone-17-pro-max",
            tagline: "Titan. Mạnh mẽ. Chuyên nghiệp.",
            basePrice: 34_990_000m,
            compareAt: 37_990_000m,
            variants: new (string Storage, string Color)[]
            {
                ("256GB", "Titan Tự Nhiên"),
                ("512GB", "Titan Tự Nhiên"),
                ("1TB", "Titan Sa Mạc")
            });

        AddProductWithVariants(
            iphone.Id,
            name: "iPhone 17",
            slug: "dien-thoai-iphone-17",
            tagline: "Sống động. Mạnh mẽ. Đáng giá.",
            basePrice: 22_990_000m,
            compareAt: 24_990_000m,
            variants: new (string Storage, string Color)[]
            {
                ("128GB", "Xanh"),
                ("256GB", "Đen")
            });

        AddProductWithVariants(
            ipad.Id,
            name: "iPad Air 11 inch M3",
            slug: "may-tinh-bang-ipad-air-11-m3",
            tagline: "Mạnh mẽ. Đa năng. Nhẹ nhàng.",
            basePrice: 16_990_000m,
            compareAt: 17_990_000m,
            variants: new (string Storage, string Color)[]
            {
                ("128GB", "Xanh Dương"),
                ("256GB", "Xám")
            });

        AddProductWithVariants(
            mac.Id,
            name: "MacBook Air M3 13 inch",
            slug: "macbook-air-m3-13",
            tagline: "Siêu mỏng. Pin cả ngày.",
            basePrice: 24_990_000m,
            compareAt: 27_990_000m,
            variants: new (string Storage, string Color)[]
            {
                ("256GB", "Xám"),
                ("512GB", "Bạc")
            });

        AddProductWithVariants(
            watch.Id,
            name: "Apple Watch Series 10",
            slug: "apple-watch-series-10",
            tagline: "Mỏng hơn. Màn lớn hơn.",
            basePrice: 10_990_000m,
            compareAt: 12_990_000m,
            variants: new (string Storage, string Color)[]
            {
                ("42mm", "Đen"),
                ("46mm", "Bạc")
            });

        AddProductWithVariants(
            phukien.Id,
            name: "AirPods Pro 2",
            slug: "airpods-pro-2",
            tagline: "Chống ồn. USB-C.",
            basePrice: 5_490_000m,
            compareAt: 6_790_000m,
            variants: new (string Storage, string Color)[]
            {
                ("USB-C", "Trắng")
            });

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Bổ sung thông số kỹ thuật cho các sản phẩm mẫu đã seed trước đây còn để rỗng
    /// (<c>SpecsJson</c> null/<c>{}</c>/<c>[]</c>). Không phá dữ liệu — chỉ điền chỗ trống.
    /// </summary>
    private async Task BackfillProductSpecsAsync()
    {
        var slugs = SpecsBySlug.Keys.ToList();
        var products = await _db.Products
            .Where(p => p.ProductParentId == null && slugs.Contains(p.Slug))
            .ToListAsync();

        var changed = false;
        foreach (var p in products)
        {
            var current = p.SpecsJson?.Trim();
            var isEmpty = string.IsNullOrEmpty(current) || current is "{}" or "[]";
            if (!isEmpty) continue;            // tôn trọng specs admin đã sửa
            p.SpecsJson = SerializeSpecs(p.Slug);
            changed = true;
        }

        if (changed) await _db.SaveChangesAsync();
    }

    private async Task SeedBannersAsync()
    {
        if (await _db.Banners.AnyAsync()) return;

        _db.Banners.AddRange(
            new Banner
            {
                Eyebrow = "Vừa ra mắt",
                Title = "iPhone 17 Pro Max",
                Subtitle = "Titan. Mạnh mẽ. Chuyên nghiệp.",
                ImageUrl = "/images/placeholder.svg",
                LinkUrl = "/dien-thoai-iphone-17-pro-max-256gb",
                CtaText = "Mua ngay",
                SortOrder = 0,
                IsActive = true
            },
            new Banner
            {
                Eyebrow = "Trả góp 0%",
                Title = "Lên đời Mac dễ dàng",
                Subtitle = "Duyệt nhanh trong 5 phút, 0đ trả trước.",
                ImageUrl = "/images/placeholder.svg",
                LinkUrl = "/mac",
                CtaText = "Khám phá",
                SortOrder = 1,
                IsActive = true
            },
            new Banner
            {
                Eyebrow = "Thu cũ đổi mới",
                Title = "Trợ giá đến 3 triệu",
                Subtitle = "Mang máy cũ đến đổi, lên đời tiết kiệm hơn.",
                ImageUrl = "/images/placeholder.svg",
                LinkUrl = "/apple-watch",
                CtaText = "Định giá máy",
                SortOrder = 2,
                IsActive = true
            });

        await _db.SaveChangesAsync();
    }

    private async Task SeedBundlesAsync()
    {
        if (await _db.Bundles.AnyAsync()) return;

        var iphone = await _db.Products.FirstOrDefaultAsync(p => p.Slug == "dien-thoai-iphone-17-pro-max");
        var airpods = await _db.Products.FirstOrDefaultAsync(v => v.Slug == "airpods-pro-2-usb-c");
        if (iphone is null || airpods is null) return;

        var bundle = new Bundle { MainProductId = iphone.Id };
        // AirPods niêm yết 5.490.000 (−10% KM = 4.941.000) → giá mua kèm 4.490.000.
        bundle.Items.Add(new BundleItem { AccessoryProductId = airpods.Id, BundlePrice = 4_490_000m });
        _db.Bundles.Add(bundle);
        await _db.SaveChangesAsync();
    }

    private async Task SeedStockDemoAsync()
    {
        // Demo "theo dõi hàng về": đảm bảo có ≥1 biến thể (Product con) hết hàng.
        if (await _db.Products.AnyAsync(v => v.Status == ProductStatus.OutOfStock)) return;

        var variant = await _db.Products.FirstOrDefaultAsync(v => v.Slug == "dien-thoai-iphone-17-256gb");
        if (variant is null) return;
        variant.Status = ProductStatus.OutOfStock;
        await _db.SaveChangesAsync();
    }

    private AttributeEntity _storageAttr = null!;
    private AttributeEntity _colorAttr = null!;

    /// <summary>
    /// Thông số kỹ thuật mẫu theo slug (nguồn duy nhất) — dùng cho cả seed mới lẫn backfill.
    /// Serialize theo contract <c>ProductSpecs</c> (mảng phẳng <c>[{label,value}]</c>).
    /// </summary>
    private static readonly Dictionary<string, (string Label, string Value)[]> SpecsBySlug = new()
    {
        ["dien-thoai-iphone-17-pro-max"] = new[]
        {
            ("Màn hình", "6.9\" Super Retina XDR, 120Hz ProMotion"),
            ("Chip", "Apple A18 Pro"),
            ("Camera sau", "48MP Fusion + Tele 5x + Ultra Wide"),
            ("Camera trước", "12MP TrueDepth"),
            ("Pin", "Đến 33 giờ phát video"),
            ("Kết nối", "USB-C, 5G, Wi-Fi 7"),
            ("Chất liệu", "Khung Titanium"),
        },
        ["dien-thoai-iphone-17"] = new[]
        {
            ("Màn hình", "6.1\" Super Retina XDR"),
            ("Chip", "Apple A18"),
            ("Camera sau", "48MP Fusion + Ultra Wide"),
            ("Camera trước", "12MP TrueDepth"),
            ("Pin", "Đến 22 giờ phát video"),
            ("Kết nối", "USB-C, 5G, Wi-Fi 6E"),
        },
        ["may-tinh-bang-ipad-air-11-m3"] = new[]
        {
            ("Màn hình", "11\" Liquid Retina, 60Hz"),
            ("Chip", "Apple M3"),
            ("Camera sau", "12MP Wide"),
            ("Camera trước", "12MP Ultra Wide (Center Stage)"),
            ("Kết nối", "USB-C, Wi-Fi 6E, hỗ trợ Apple Pencil Pro"),
            ("Pin", "Đến 10 giờ lướt web"),
        },
        ["macbook-air-m3-13"] = new[]
        {
            ("Màn hình", "13.6\" Liquid Retina, 500 nits"),
            ("Chip", "Apple M3 (8 nhân CPU, 10 nhân GPU)"),
            ("RAM", "16GB bộ nhớ hợp nhất"),
            ("Pin", "Đến 18 giờ"),
            ("Cổng", "2x Thunderbolt/USB 4, MagSafe 3, jack 3.5mm"),
            ("Trọng lượng", "1.24 kg"),
        },
        ["apple-watch-series-10"] = new[]
        {
            ("Màn hình", "Always-On Retina LTPO3 OLED"),
            ("Chip", "Apple S10 SiP"),
            ("Sức khỏe", "ECG, SpO2, nhịp tim, nhiệt độ cổ tay"),
            ("Chống nước", "WR50 (50m)"),
            ("Pin", "Đến 18 giờ (36 giờ chế độ tiết kiệm)"),
        },
        ["airpods-pro-2"] = new[]
        {
            ("Chip", "Apple H2"),
            ("Chống ồn", "Khử ồn chủ động (ANC) thế hệ mới"),
            ("Âm thanh", "Adaptive Audio, Spatial Audio"),
            ("Pin", "Đến 6 giờ (30 giờ với hộp sạc)"),
            ("Chống nước", "IP54 (tai nghe & hộp sạc)"),
            ("Sạc", "USB-C, MagSafe, Qi"),
        },
    };

    private static string SerializeSpecs(string slug)
        => SpecsBySlug.TryGetValue(slug, out var s) && s.Length > 0
            ? System.Text.Json.JsonSerializer.Serialize(s.Select(x => new { label = x.Label, value = x.Value }))
            : "[]";

    private void AddProductWithVariants(
        long categoryId, string name, string slug, string tagline, decimal basePrice, decimal compareAt,
        (string Storage, string Color)[] variants)
    {
        var now = DateTime.Now;
        var specsJson = SerializeSpecs(slug);

        // Model cha (gom + ảnh gallery; không bán trực tiếp → giá 0, không SKU).
        var parent = new Product
        {
            CategoryId = categoryId,
            Name = name,
            Slug = slug,
            Tagline = tagline,
            Brand = "Apple",
            Description = $"{name} chính hãng Apple, nguyên seal 100%, đầy đủ phụ kiện. Bảo hành 12 tháng, " +
                          "hỗ trợ trả góp 0% và thu cũ đổi mới.",
            SpecsJson = specsJson,
            Status = ProductStatus.Active
        };
        parent.Images.Add(new ProductImage { Url = "/images/placeholder.svg", SortOrder = 0 });

        var i = 0;
        foreach (var (storage, color) in variants)
        {
            // Biến thể = Product con; thuộc tính Dung lượng/Màu lưu ở ProductAttribute.
            var child = new Product
            {
                CategoryId = categoryId,
                Name = name,
                Slug = $"{slug}-{storage.ToLowerInvariant()}",
                Sku = $"{slug}-{storage}".ToUpperInvariant(),
                BasePrice = basePrice + i * 4_000_000m,
                CompareAtPrice = compareAt + i * 4_000_000m,
                Status = ProductStatus.Active
            };
            child.Attributes.Add(new ProductAttribute { Attribute = _storageAttr, Value = storage, CreatedDate = now });
            child.Attributes.Add(new ProductAttribute { Attribute = _colorAttr, Value = color, CreatedDate = now });
            parent.Children.Add(child);
            i++;
        }

        _db.Products.Add(parent);
    }
}
