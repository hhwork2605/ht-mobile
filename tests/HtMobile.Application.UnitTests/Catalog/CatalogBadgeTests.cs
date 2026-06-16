using FluentAssertions;
using HtMobile.Application.Features.Catalog;
using Xunit;

namespace HtMobile.Application.UnitTests.Catalog;

public class CatalogBadgeTests
{
    private static readonly DateTime Now = new(2026, 6, 16, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Created_today_is_new()
    {
        CatalogBadge.IsNew(Now, Now).Should().BeTrue();
    }

    [Fact]
    public void Created_within_window_is_new()
    {
        CatalogBadge.IsNew(Now.AddDays(-10), Now).Should().BeTrue();
    }

    [Fact]
    public void Created_before_window_is_not_new()
    {
        CatalogBadge.IsNew(Now.AddDays(-40), Now).Should().BeFalse();
    }
}
