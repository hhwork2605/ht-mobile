using HtMobile.Domain.Entities.Catalog;
using HtMobile.Domain.Entities.Cms;
using HtMobile.Domain.Entities.Customers;
using HtMobile.Domain.Entities.Localization;
using HtMobile.Domain.Entities.Pricing;
using HtMobile.Domain.Entities.Reviews;
using HtMobile.Domain.Entities.Sales;
using HtMobile.Domain.Entities.Services;
using Microsoft.EntityFrameworkCore;
using AttributeEntity = HtMobile.Domain.Entities.Catalog.Attribute;
using InventoryEntity = HtMobile.Domain.Entities.Inventory.Inventory;
using StoreEntity = HtMobile.Domain.Entities.Inventory.Store;

namespace HtMobile.Application.Common.Interfaces;

/// <summary>
/// Cổng truy cập DB cho lớp Application (chỉ EF Core abstractions, không lộ provider).
/// Infrastructure.AppDbContext hiện thực interface này.
/// </summary>
public interface IApplicationDbContext
{
    // Catalog
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<AttributeEntity> Attributes { get; }
    DbSet<ProductAttribute> ProductAttributes { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductVideo> ProductVideos { get; }

    // Pricing
    DbSet<Promotion> Promotions { get; }
    DbSet<PaymentPromotion> PaymentPromotions { get; }

    // Inventory
    DbSet<StoreEntity> Stores { get; }
    DbSet<InventoryEntity> Inventories { get; }

    // Sales
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Shipment> Shipments { get; }
    DbSet<Bundle> Bundles { get; }
    DbSet<BundleItem> BundleItems { get; }

    // Customers / Reviews
    DbSet<Address> Addresses { get; }
    DbSet<Review> Reviews { get; }

    // CMS
    DbSet<Article> Articles { get; }
    DbSet<Page> Pages { get; }
    DbSet<Banner> Banners { get; }

    // Services
    DbSet<ServicePackage> ServicePackages { get; }
    DbSet<TradeInRequest> TradeInRequests { get; }
    DbSet<StockNotification> StockNotifications { get; }

    // Localization
    DbSet<Translation> Translations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
