﻿using DigitalSeal.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DigitalSeal.Data.Interceptors
{
    public class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        private readonly TimeProvider _dateTime;

        public AuditableEntityInterceptor(TimeProvider dateTime)
        {
            _dateTime = dateTime;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateEntities(eventData.Context);

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateEntities(eventData.Context);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public void UpdateEntities(DbContext? context)
        {
            if (context == null) return;

            foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
            {
                if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
                {
                    var utcNow = _dateTime.GetUtcNow().UtcDateTime;
                    if (entry.State == EntityState.Added)
                        entry.Entity.Created = utcNow;
                    entry.Entity.LastModified = utcNow;
                }
            }
        }
    }

    public static class Extensions
    {
        public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
            entry.References.Any(r =>
                r.TargetEntry != null &&
                r.TargetEntry.Metadata.IsOwned() &&
                (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
    }
}
