using HtMobile.Domain.Entities.Pricing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HtMobile.Infrastructure.Persistence.Configurations;

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
