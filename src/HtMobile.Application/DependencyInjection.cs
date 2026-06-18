using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Bundles;
using HtMobile.Application.Features.Cart;
using HtMobile.Application.Features.Catalog;
using HtMobile.Application.Features.Checkout;
using HtMobile.Application.Features.Dashboard;
using HtMobile.Application.Features.Inventory;
using HtMobile.Application.Features.Orders;
using HtMobile.Application.Features.Pricing;
using HtMobile.Application.Features.Reports;
using HtMobile.Application.Features.Seo;
using HtMobile.Application.Features.StockNotifications;
using HtMobile.Application.Features.TradeIn;
using Microsoft.Extensions.DependencyInjection;

namespace HtMobile.Application;

/// <summary>Đăng ký service của lớp Application. Gọi từ Program.cs (composition root).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPricingService, PricingEngine>();
        services.AddScoped<AdminPromotionService>();
        services.AddScoped<CatalogService>();
        services.AddScoped<AdminProductService>();
        services.AddScoped<AdminCategoryService>();
        services.AddScoped<AdminInventoryService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<CheckoutService>();
        services.AddScoped<OrderHistoryService>();
        services.AddScoped<AdminOrderService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<ReportsService>();
        services.AddScoped<AdminSeoService>();
        services.AddScoped<BundleService>();
        services.AddScoped<TradeInService>();
        services.AddScoped<StockNotificationService>();

        // TODO (vibe-code): đăng ký thêm service của từng feature khi triển khai
        // (Cart, Orders, Checkout, Promotions, TradeIn, …)

        return services;
    }
}
