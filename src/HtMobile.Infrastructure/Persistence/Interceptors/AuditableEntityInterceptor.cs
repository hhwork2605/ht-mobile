using HtMobile.Application.Common.Interfaces;
using HtMobile.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HtMobile.Infrastructure.Persistence.Interceptors;

/// <summary>Tự gán CreatedAt/UpdatedAt cho <see cref="BaseAuditableEntity"/> khi SaveChanges.</summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IDateTime _clock;

    public AuditableEntityInterceptor(IDateTime clock) => _clock = clock;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void Apply(DbContext? context)
    {
        if (context is null) return;
        var now = _clock.Now;

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
    }
}
