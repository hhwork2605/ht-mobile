using FluentAssertions;
using HtMobile.Domain.Entities.Cms;
using Xunit;

namespace HtMobile.Application.UnitTests.Catalog;

public class BannerTests
{
    private static readonly DateTime Now = new(2026, 6, 16, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Active_with_no_window_is_shown()
    {
        var banner = new Banner { Title = "Hero", IsActive = true };

        banner.IsActiveAt(Now).Should().BeTrue();
    }

    [Fact]
    public void Inactive_flag_is_hidden_even_inside_window()
    {
        var banner = new Banner
        {
            Title = "Hero",
            IsActive = false,
            StartsAt = Now.AddDays(-1),
            EndsAt = Now.AddDays(1)
        };

        banner.IsActiveAt(Now).Should().BeFalse();
    }

    [Fact]
    public void Inside_window_is_shown()
    {
        var banner = new Banner { Title = "Hero", IsActive = true, StartsAt = Now.AddDays(-1), EndsAt = Now.AddDays(1) };

        banner.IsActiveAt(Now).Should().BeTrue();
    }

    [Fact]
    public void Before_start_is_hidden()
    {
        var banner = new Banner { Title = "Hero", IsActive = true, StartsAt = Now.AddDays(1) };

        banner.IsActiveAt(Now).Should().BeFalse();
    }

    [Fact]
    public void After_end_is_hidden()
    {
        var banner = new Banner { Title = "Hero", IsActive = true, EndsAt = Now.AddDays(-1) };

        banner.IsActiveAt(Now).Should().BeFalse();
    }
}
