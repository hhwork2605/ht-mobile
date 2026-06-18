using FluentAssertions;
using HtMobile.Application.Features.Catalog;
using Xunit;

namespace HtMobile.Application.UnitTests.Catalog;

public class VariantAxisBuilderTests
{
    // 4 biến thể: (128/256GB) × (Đen/Xanh)
    private static VariantInfo V(long id, string slug, string storage, string color, bool active = true) =>
        new(id, slug, active, new[]
        {
            new VariantAttr("Dung lượng", 1, storage),
            new VariantAttr("Màu", 2, color),
        });

    private static readonly VariantInfo[] Matrix =
    {
        V(1, "p-128-den", "128GB", "Đen"),
        V(2, "p-128-xanh", "128GB", "Xanh"),
        V(3, "p-256-den", "256GB", "Đen"),
        V(4, "p-256-xanh", "256GB", "Xanh"),
    };

    [Fact]
    public void Builds_one_axis_per_attribute_ordered_by_sortorder()
    {
        var axes = VariantAxisBuilder.Build(Matrix, selectedId: 1);

        axes.Select(a => a.Name).Should().ContainInOrder("Dung lượng", "Màu");
        axes[0].Options.Select(o => o.Value).Should().Equal("128GB", "256GB");
        axes[1].Options.Select(o => o.Value).Should().Equal("Đen", "Xanh");
    }

    [Fact]
    public void Marks_selected_values_on_each_axis()
    {
        var axes = VariantAxisBuilder.Build(Matrix, selectedId: 3); // 256GB · Đen

        axes.Single(a => a.Name == "Dung lượng").Options.Single(o => o.IsSelected).Value.Should().Be("256GB");
        axes.Single(a => a.Name == "Màu").Options.Single(o => o.IsSelected).Value.Should().Be("Đen");
    }

    [Fact]
    public void Switching_one_axis_keeps_other_axis_fixed()
    {
        // Đang chọn 128GB·Đen (id 1). Bấm màu "Xanh" → phải tới 128GB·Xanh (id 2), giữ nguyên dung lượng.
        var axes = VariantAxisBuilder.Build(Matrix, selectedId: 1);

        var xanh = axes.Single(a => a.Name == "Màu").Options.Single(o => o.Value == "Xanh");
        xanh.Slug.Should().Be("p-128-xanh");
        xanh.Available.Should().BeTrue();
    }

    [Fact]
    public void Inactive_target_marked_unavailable()
    {
        var variants = new[]
        {
            V(1, "p-128-den", "128GB", "Đen"),
            V(2, "p-256-den", "256GB", "Đen", active: false),
        };
        var axes = VariantAxisBuilder.Build(variants, selectedId: 1);

        var opt256 = axes.Single(a => a.Name == "Dung lượng").Options.Single(o => o.Value == "256GB");
        opt256.Available.Should().BeFalse();
        opt256.Slug.Should().Be("p-256-den"); // vẫn có slug để điều hướng/hiển thị
    }

    [Fact]
    public void Empty_input_yields_no_axes()
        => VariantAxisBuilder.Build(Array.Empty<VariantInfo>(), 0).Should().BeEmpty();
}
