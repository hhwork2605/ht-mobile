using FluentAssertions;
using HtMobile.Application.Features.TradeIn;
using HtMobile.Domain.Enums;
using HtMobile.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HtMobile.Infrastructure.IntegrationTests.TradeIn;

/// <summary>TradeInService trên EF InMemory: tạo yêu cầu với giá tính SERVER-SIDE; key lạ → không tạo.</summary>
public class TradeInServiceTests
{
    private static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("tradein-" + Guid.NewGuid().ToString("N"))
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Submit_creates_request_with_server_computed_estimate()
    {
        using var db = NewDb();
        var svc = new TradeInService(db);

        // iPhone 13 base 7.000.000 × 0.9 (Tốt) = 6.300.000 + trợ giá 500.000 = 6.800.000
        var result = await svc.SubmitAsync(customerId: 42, deviceKey: "iphone-13", conditionKey: "tot");

        result.Should().NotBeNull();
        result!.Quote.Total.Should().Be(6_800_000m);

        var req = await db.TradeInRequests.SingleAsync();
        req.CustomerId.Should().Be(42);
        req.Model.Should().Be("iPhone 13");
        req.EstimatedPrice.Should().Be(6_800_000m);   // = quote server
        req.Status.Should().Be(TradeInStatus.Pending);
    }

    [Fact]
    public async Task Guest_submit_has_null_customer()
    {
        using var db = NewDb();
        var result = await new TradeInService(db).SubmitAsync(null, "iphone-13", "tot");

        result.Should().NotBeNull();
        (await db.TradeInRequests.SingleAsync()).CustomerId.Should().BeNull();
    }

    [Theory]
    [InlineData("khong-co", "tot")]
    [InlineData("iphone-13", "xxx")]
    public async Task Invalid_keys_create_no_request(string deviceKey, string conditionKey)
    {
        using var db = NewDb();
        var result = await new TradeInService(db).SubmitAsync(1, deviceKey, conditionKey);

        result.Should().BeNull();
        (await db.TradeInRequests.CountAsync()).Should().Be(0);
    }
}
