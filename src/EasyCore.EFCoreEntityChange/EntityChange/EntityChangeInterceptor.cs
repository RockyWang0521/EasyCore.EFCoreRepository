using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.EFCoreEntityChange.EntityChange
{
    /// <summary>
    /// Entity change interceptor.
    /// </summary>
    internal class EntityChangeInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceProvider _serviceProvider;

        public EntityChangeInterceptor(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
               DbContextEventData eventData,
               InterceptionResult<int> result,
               CancellationToken cancellationToken = default)
        {
            var context = eventData.Context!;

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
            {
                var entityType = entry.Entity.GetType();

                if (entry.State == EntityState.Added)
                {
                    var handlerType = typeof(IEntityAddedChangeHandler<>).MakeGenericType(entityType);

                    var handlers = _serviceProvider.GetServices(handlerType);


                    foreach (var handler in handlers)
                    {
                        try
                        {
                            await (Task)handlerType
                                .GetMethod(nameof(IEntityAddedChangeHandler<object>.OnAddedAsync))!
                                .Invoke(handler, new[] { entry.Entity })!;
                        }
                        catch
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"fail: {DateTime.Now} Error while invoking entity added change handler.");
                            Console.ResetColor();
                        }
                    }
                }

                if (entry.State == EntityState.Modified)
                {
                    var handlerType = typeof(IEntityUpdatedChangeHandler<,>).MakeGenericType(entityType, entityType);

                    var handlers = _serviceProvider.GetServices(handlerType);

                    foreach (var handler in handlers)
                    {
                        var currentEntity = entry.Entity;

                        var originalEntity = entry.OriginalValues.ToObject();

                        try
                        {
                            await (Task)handlerType
                               .GetMethod(nameof(IEntityUpdatedChangeHandler<object, object>.OnUpdatedAsync))!
                               .Invoke(handler, new[] { originalEntity, currentEntity })!;
                        }
                        catch
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"fail: {DateTime.Now} Error while invoking entity updated change handler.");
                            Console.ResetColor();
                        }
                    }
                }

                if (entry.State == EntityState.Deleted)
                {
                    var handlerType = typeof(IEntityDeletedChangeHandler<>).MakeGenericType(entityType);

                    var handlers = _serviceProvider.GetServices(handlerType);

                    foreach (var handler in handlers)
                    {
                        try
                        {
                            await (Task)handlerType
                                .GetMethod(nameof(IEntityDeletedChangeHandler<object>.OnDeletedAsync))!
                                .Invoke(handler, new[] { entry.Entity })!;
                        }
                        catch
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"fail: {DateTime.Now} Error while invoking entity deleted change handler.");
                            Console.ResetColor();
                        }
                    }
                }
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
