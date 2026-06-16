using HtMobile.Domain.Constants;
using HtMobile.Domain.Entities.Catalog;
using HtMobile.Domain.Entities.Cms;
using HtMobile.Domain.Entities.Pricing;
using HtMobile.Domain.Enums;
using HtMobile.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HtMobile.Infrastructure.Persistence.Seed;

/// <summary>Apply migration + seed dữ liệu mẫu. Gọi lúc khởi động ở Development.</summary>
public class DbInitializer
{
    public const string AdminEmail = "admin@htmobile.local";
    public const string AdminPassword = "Admin@123456";

    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole<long>> _roles;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(
        AppDbContext db,
        UserManager<ApplicationUser> users,
        RoleManager<IdentityRole<long>> roles,
        ILogger<DbInitializer> logger)
    {
        _db = db;
        _users = users;
        _roles = roles;
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
        var regions = await SeedRegionsAsync();
        await SeedCategoriesAsync();
        await SeedSampleCatalogAsync(regions);
        await SeedBannersAsync();
        _logger.LogInformation("Seed completed.");
    }

    private async Task SeedRolesAndAdminAsync()
    {
        foreach (var role in Roles.All)
            if (!await _roles.RoleExistsAsync(role))
                await _roles.CreateAsync(new IdentityRole<long>(role));

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

    private async Task<(Region North, Region South)> SeedRegionsAsync()
    {
        var north = await _db.Regions.FirstOrDefaultAsync(r => r.Code == "north");
        var south = await _db.Regions.FirstOrDefaultAsync(r => r.Code == "south");

        if (north is null)
        {
            north = new Region { Name = "Miền Bắc", Code = "north" };
            _db.Regions.Add(north);
        }
        if (south is null)
        {
            south = new Region { Name = "Miền Nam", Code = "south" };
            _db.Regions.Add(south);
        }
        await _db.SaveChangesAsync();
        return (north, south);
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

    private async Task SeedSampleCatalogAsync((Region North, Region South) regions)
    {
        if (await _db.Products.AnyAsync()) return;

        var now = DateTime.Now;
        var iphone = await _db.Categories.FirstAsync(c => c.Slug == "iphone");
        var ipad = await _db.Categories.FirstAsync(c => c.Slug == "ipad");
        var mac = await _db.Categories.FirstAsync(c => c.Slug == "mac");
        var watch = await _db.Categories.FirstAsync(c => c.Slug == "apple-watch");
        var phukien = await _db.Categories.FirstAsync(c => c.Slug == "phu-kien");

        // Khuyến mãi 10% áp cho mọi sản phẩm (scaffold)
        _db.Promotions.Add(new Promotion
        {
            Name = "Ưu đãi khai trương -10%",
            Type = PromotionType.Percentage,
            Value = 10,
            StartsAt = now.AddDays(-1),
            EndsAt = now.AddMonths(1)
        });

        AddProductWithVariants(
            iphone.Id,
            name: "iPhone 17 Pro Max",
            slug: "dien-thoai-iphone-17-pro-max",
            tagline: "Titan. Mạnh mẽ. Chuyên nghiệp.",
            basePrice: 34_990_000m,
            compareAt: 37_990_000m,
            regions,
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
            regions,
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
            regions,
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
            regions,
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
            regions,
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
            regions,
            variants: new (string Storage, string Color)[]
            {
                ("USB-C", "Trắng")
            });

        await _db.SaveChangesAsync();
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

    private void AddProductWithVariants(
        long categoryId, string name, string slug, string tagline, decimal basePrice, decimal compareAt,
        (Region North, Region South) regions, (string Storage, string Color)[] variants)
    {
        var product = new Product
        {
            CategoryId = categoryId,
            Name = name,
            Slug = slug,
            Tagline = tagline,
            Brand = "Apple",
            Description = $"{name} chính hãng Apple, nguyên seal 100%, đầy đủ phụ kiện. Bảo hành 12 tháng, " +
                          "hỗ trợ trả góp 0% và thu cũ đổi mới.",
            SpecsJson = "{}"
        };
        product.Images.Add(new ProductImage { Url = "/images/placeholder.svg", SortOrder = 0 });

        var i = 0;
        foreach (var (storage, color) in variants)
        {
            var price = basePrice + i * 4_000_000m;
            var variant = new ProductVariant
            {
                Sku = $"{slug}-{storage}".ToUpperInvariant(),
                Storage = storage,
                Color = color,
                Slug = $"{slug}-{storage.ToLowerInvariant()}",
                BasePrice = price,
                Status = VariantStatus.Active
            };
            variant.Prices.Add(new PriceByRegion { Region = regions.North, Price = price, CompareAtPrice = compareAt + i * 4_000_000m });
            variant.Prices.Add(new PriceByRegion { Region = regions.South, Price = price + 200_000m, CompareAtPrice = compareAt + i * 4_000_000m + 200_000m });
            product.Variants.Add(variant);
            i++;
        }

        _db.Products.Add(product);
    }
}
