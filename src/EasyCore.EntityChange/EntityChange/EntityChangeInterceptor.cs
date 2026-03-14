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
    /// </summary>
    public class EntityChangeInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EntityChangeInterceptor> _logger;
        private readonly EntityChangeOptions _options;

        public EntityChangeInterceptor(
            IServiceProvider serviceProvider,
            ILogger<EntityChangeInterceptor> logger,
            IOptions<EntityChangeOptions>? options = null)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options?.Value ?? new EntityChangeOptions();
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context
                ?? throw new InvalidOperationException("DbContext is null in SavingChangesAsync.");

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
            {
                var entityType = entry.Entity.GetType();

                if (entry.State == EntityState.Added)
                {
                    await InvokeHandlersAsync(
                        typeof(IEntityAddedChangeHandler<>).MakeGenericType(entityType),
                        nameof(IEntityAddedChangeHandler<object>.OnAddedAsync),
                        new[] { entry.Entity },
                        "added");
                    continue;
                }

                if (entry.State == EntityState.Deleted || IsSoftDeleteTransition(entry))
                {
                    await InvokeHandlersAsync(
                        typeof(IEntityDeletedChangeHandler<>).MakeGenericType(entityType),
                        nameof(IEntityDeletedChangeHandler<object>.OnDeletedAsync),
                        new[] { entry.Entity },
                        "deleted");
                    continue;
                }

                if (entry.State == EntityState.Modified)
                {
                    await InvokeHandlersAsync(
                        typeof(IEntityUpdatedChangeHandler<>).MakeGenericType(entityType),
                        nameof(IEntityUpdatedChangeHandler<object>.OnUpdatedAsync),
                        new[] { entry.OriginalValues.ToObject(), entry.Entity },
                        "updated");
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private static bool IsSoftDeleteTransition(EntityEntry entry)
        {
            if (entry.State != EntityState.Modified)
                return false;

            var prop = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "IsDeleted");
            if (prop == null)
                return false;

            return prop.OriginalValue is false && prop.CurrentValue is true;
        }

        private async Task InvokeHandlersAsync(
            Type handlerType,
            string methodName,
            object?[] args,
            string operation)
        {
            var handlers = _serviceProvider.GetServices(handlerType);

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
