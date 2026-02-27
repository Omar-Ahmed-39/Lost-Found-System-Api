using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LostAndFound.Infrastructure.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        SetAuditProperties(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetAuditProperties(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void SetAuditProperties(DbContext? context)
    {
        if (context is null) return;

        var utcNow = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Handle CreatedAt on insert
            if (entry.State == EntityState.Added)
            {
                if (entry.Properties.Any(p => p.Metadata.Name == "CreatedAt"))
                    entry.Property("CreatedAt").CurrentValue = utcNow;

                // User entity uses "Created" instead of "CreatedAt"
                if (entry.Properties.Any(p => p.Metadata.Name == "Created"))
                    entry.Property("Created").CurrentValue = utcNow;
            }

            // Handle UpdatedAt on insert and update
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                if (entry.Properties.Any(p => p.Metadata.Name == "UpdatedAt"))
                    entry.Property("UpdatedAt").CurrentValue = utcNow;
            }
        }
    }
}
