using HtMobile.Application.Common.Interfaces;
using HtMobile.Infrastructure.Caching;
using HtMobile.Infrastructure.Common;
using HtMobile.Infrastructure.Identity;
using HtMobile.Infrastructure.Integrations;
using HtMobile.Infrastructure.Persistence;
using HtMobile.Infrastructure.Persistence.Interceptors;
using HtMobile.Infrastructure.Persistence.Seed;
using HtMobile.Infrastructure.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HtMobile.Infrastructure;

/// <summary>Đăng ký hạ tầng: EF/Postgres, Identity, Redis cache, search, email. Gọi từ Program.cs.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Dùng DateTime "thuần" (không offset): map sang `timestamp without time zone` và bỏ kiểm tra Kind
        // của Npgsql, để ghi DateTime.UtcNow vào cột timestamp không lỗi. Xem docs/conventions.md.
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var connectionString = config.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=htmobile;Username=postgres;Password=postgres";

        services.AddSingleton<IDateTime, SystemDateTime>();
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention()
                   .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddIdentity<ApplicationUser, IdentityRole<long>>(options =>
        {
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.AccessDeniedPath = "/login";
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;
        });

        // Redis (cache giá/KM + session/giỏ khách vãng lai)
        var redis = config.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddStackExchangeRedisCache(o => o.Configuration = redis);
        services.AddScoped<ICacheService, RedisCacheService>();

        services.AddScoped<ISearchService, PostgresSearchService>();
        services.AddScoped<IEmailSender, NullEmailSender>();

        services.AddScoped<DbInitializer>();

        return services;
    }
}
