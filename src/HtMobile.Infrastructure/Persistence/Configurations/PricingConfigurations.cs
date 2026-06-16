using HtMobile.Domain.Entities.Pricing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HtMobile.Infrastructure.Persistence.Configurations;

public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> b)
    {
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Code).HasMaxLength(40).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
    }
}

public class PriceByRegionConfiguration : IEntityTypeConfiguration<PriceByRegion>
{
    public void Configure(EntityTypeBuilder<PriceByRegion> b)
    {
        b.HasIndex(x => new { x.VariantId, x.RegionId }).IsUnique();

        b.HasOne(x => x.Variant)
            .WithMany(x => x.Prices)
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Region)
            .WithMany(x => x.Prices)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> b)
    {
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.ConditionsJson).HasColumnType("jsonb");
        b.HasIndex(x => new { x.StartsAt, x.EndsAt });
    }
}

public class PaymentPromotionConfiguration : IEntityTypeConfiguration<PaymentPromotion>
{
    public void Configure(EntityTypeBuilder<PaymentPromotion> b)
    {
        b.Property(x => x.Bank).HasMaxLength(100).IsRequired();
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
    }
}
