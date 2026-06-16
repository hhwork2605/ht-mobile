using HtMobile.Application.Common.Interfaces;
using HtMobile.Domain.Entities.Catalog;
using HtMobile.Domain.Entities.Cms;
using HtMobile.Domain.Entities.Customers;
using HtMobile.Domain.Entities.Localization;
using HtMobile.Domain.Entities.Pricing;
using HtMobile.Domain.Entities.Reviews;
using HtMobile.Domain.Entities.Sales;
using HtMobile.Domain.Entities.Services;
using HtMobile.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InventoryEntity = HtMobile.Domain.Entities.Inventory.Inventory;
using StoreEntity = HtMobile.Domain.Entities.Inventory.Store;

namespace HtMobile.Infrastructure.Persistence;

/// <summary>
/// DbContext gốc: kết hợp ASP.NET Identity (khóa <see cref="long"/>) và <see cref="IApplicationDbContext"/>.
/// snake_case áp dụng ở tầng DI qua <c>UseSnakeCaseNamingConvention()</c>.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<long>, long>, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Catalog
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVideo> ProductVideos => Set<ProductVideo>();

    // Pricing
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<PriceByRegion> PricesByRegion => Set<PriceByRegion>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<PaymentPromotion> PaymentPromotions => Set<PaymentPromotion>();

    // Inventory
    public DbSet<StoreEntity> Stores => Set<StoreEntity>();
    public DbSet<InventoryEntity> Inventories => Set<InventoryEntity>();

    // Sales
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<Bundle> Bundles => Set<Bundle>();
    public DbSet<BundleItem> BundleItems => Set<BundleItem>();

    // Customers / Reviews
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Review> Reviews => Set<Review>();

    // CMS
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<Banner> Banners => Set<Banner>();

    // Services
    public DbSet<ServicePackage> ServicePackages => Set<ServicePackage>();
    public DbSet<TradeInRequest> TradeInRequests => Set<TradeInRequest>();
    public DbSet<StockNotification> StockNotifications => Set<StockNotification>();

    // Localization
    public DbSet<Translation> Translations => Set<Translation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Extension cho full-text/trigram (autocomplete)
        builder.HasPostgresExtension("pg_trgm");

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Mặc định cho mọi cột decimal (tiền tệ)
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }
}
