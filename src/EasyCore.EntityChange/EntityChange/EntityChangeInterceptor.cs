using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyCore.EntityChange.EntityChange
{
    /// <summary>
    /// Entity change interceptor that dispatches typed handlers on SaveChanges.
    /// Soft-delete transitions (IsDeleted: false → true) are treated as Deleted.
    /// Handles both sync <see cref="SavingChanges"/> and async <see cref="SavingChangesAsync"/>.
    /// </summary>
    public class EntityChangeInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EntityChangeInterceptor> _logger;
        private readonly EntityChangeOptions _options;

        public EntityChangeInterceptor(
            IServiceScopeFactory scopeFactory,
            ILogger<EntityChangeInterceptor> logger,
            IOptions<EntityChangeOptions>? options = null)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options?.Value ?? new EntityChangeOptions();
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            DispatchHandlersAsync(eventData).GetAwaiter().GetResult();
            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            await DispatchHandlersAsync(eventData);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private async Task DispatchHandlersAsync(DbContextEventData eventData)
        {
            var context = eventData.Context
                ?? throw new InvalidOperationException("DbContext is null in SavingChanges.");

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            if (entries.Count == 0)
                return;

            // Options often capture this interceptor as a singleton; resolve handlers per call.
            await using var scope = _scopeFactory.CreateAsyncScope();
            var services = scope.ServiceProvider;

            foreach (var entry in entries)
            {
                var entityType = entry.Entity.GetType();

                if (entry.State == EntityState.Added)
                {
                    await InvokeHandlersAsync(
                        services,
                        typeof(IEntityAddedChangeHandler<>).MakeGenericType(entityType),
                        nameof(IEntityAddedChangeHandler<object>.OnAddedAsync),
                        new[] { entry.Entity },
                        "added");
                    continue;
                }

                if (entry.State == EntityState.Deleted || IsSoftDeleteTransition(entry))
                {
                    await InvokeHandlersAsync(
                        services,
                        typeof(IEntityDeletedChangeHandler<>).MakeGenericType(entityType),
                        nameof(IEntityDeletedChangeHandler<object>.OnDeletedAsync),
                        new[] { entry.Entity },
                        "deleted");
                    continue;
                }

                if (entry.State == EntityState.Modified)
                {
                    await InvokeHandlersAsync(
                        services,
                        typeof(IEntityUpdatedChangeHandler<>).MakeGenericType(entityType),
                        nameof(IEntityUpdatedChangeHandler<object>.OnUpdatedAsync),
                        new[] { entry.OriginalValues.ToObject(), entry.Entity },
                        "updated");
                }
            }
        }

        private static bool IsSoftDeleteTransition(EntityEntry entry)
        {
            if (entry.State != EntityState.Modified)
                return false;

            var prop = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "IsDeleted");
            if (prop == null)
                return false;

            // Mongo EF / some providers may leave OriginalValue unset; treat non-true as "was active".
            var currentDeleted = prop.CurrentValue is true;
            var originalDeleted = prop.OriginalValue is true;
            return currentDeleted && !originalDeleted;
        }

        private async Task InvokeHandlersAsync(
            IServiceProvider services,
            Type handlerType,
            string methodName,
            object?[] args,
            string operation)
        {
            var handlers = services.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                try
                {
                    var task = (Task?)handlerType.GetMethod(methodName)!.Invoke(handler, args);
                    if (task != null)
                        await task;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while invoking entity {Operation} change handler.", operation);

                    if (!_options.SuppressHandlerExceptions)
                        throw;
                }
            }
        }
    }
}
