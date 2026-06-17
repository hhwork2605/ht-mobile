using HtMobile.Application.Common.Interfaces;
using HtMobile.Application.Features.Bundles;
using HtMobile.Application.Features.Cart;
using HtMobile.Application.Features.Catalog;
using HtMobile.Application.Features.Checkout;
using HtMobile.Application.Features.Orders;
using HtMobile.Application.Features.Pricing;
using Microsoft.Extensions.DependencyInjection;

namespace HtMobile.Application;

/// <summary>Đăng ký service của lớp Application. Gọi từ Program.cs (composition root).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPricingService, PricingEngine>();
        services.AddScoped<CatalogService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<CheckoutService>();
        services.AddScoped<OrderHistoryService>();
        services.AddScoped<BundleService>();

        // TODO (vibe-code): đăng ký thêm service của từng feature khi triển khai
        // (Cart, Orders, Checkout, Promotions, TradeIn, …)

        return services;
    }
}
