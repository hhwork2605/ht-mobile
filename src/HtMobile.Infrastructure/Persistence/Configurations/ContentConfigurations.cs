using HtMobile.Domain.Entities.Cms;
using HtMobile.Domain.Entities.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HtMobile.Infrastructure.Persistence.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> b)
    {
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(300).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();
    }
}

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> b)
    {
        b.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();
    }
}

public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> b)
    {
        b.Property(x => x.Eyebrow).HasMaxLength(80);
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Subtitle).HasMaxLength(300);
        b.Property(x => x.ImageUrl).HasMaxLength(500);
        b.Property(x => x.LinkUrl).HasMaxLength(500);
        b.Property(x => x.CtaText).HasMaxLength(60);
        b.HasIndex(x => new { x.IsActive, x.SortOrder });
    }
}

public class TranslationConfiguration : IEntityTypeConfiguration<Translation>
{
    public void Configure(EntityTypeBuilder<Translation> b)
    {
        b.Property(x => x.Entity).HasMaxLength(80).IsRequired();
        b.Property(x => x.Lang).HasMaxLength(8).IsRequired();
        b.Property(x => x.Field).HasMaxLength(80).IsRequired();
        b.HasIndex(x => new { x.Entity, x.EntityId, x.Lang, x.Field }).IsUnique();
    }
}
