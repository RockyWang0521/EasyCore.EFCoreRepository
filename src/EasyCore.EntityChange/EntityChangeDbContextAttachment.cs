using EasyCore.EntityChange.EntityChange;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.EntityChange;

/// <summary>
/// Attaches <see cref="EntityChangeInterceptor"/> to registered
/// <see cref="DbContextOptions{TContext}"/> so callers do not need <c>UseEasyCoreEntityChange</c>.
/// </summary>
internal static class EntityChangeDbContextAttachment
{
    private static readonly object Sync = new();

    /// <summary>
    /// Wraps every <see cref="DbContextOptions{TContext}"/> registration so the interceptor is added
    /// when options are built. Safe to call multiple times (idempotent per context type).
    /// </summary>
    public static void AttachAll(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!services.Any(d => d.ServiceType == typeof(EntityChangeInterceptor)))
        {
            return;
        }

        var contextTypes = services
            .Select(d => d.ServiceType)
            .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(DbContextOptions<>))
            .Select(t => t.GetGenericArguments()[0])
            .Distinct()
            .ToList();

        foreach (var contextType in contextTypes)
        {
            typeof(EntityChangeDbContextAttachment)
                .GetMethod(nameof(AttachCore), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(contextType)
                .Invoke(null, new object[] { services });
        }
    }

    private static void AttachCore<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        lock (Sync)
        {
            if (services.Any(d => d.ServiceType == typeof(EntityChangeAttachmentMarker<TContext>)))
            {
                return;
            }

            var optionsDescriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>))
                .ToList();

            if (optionsDescriptors.Count == 0)
            {
                return;
            }

            foreach (var descriptor in optionsDescriptors)
            {
                services.Remove(descriptor);
                services.Add(Wrap<TContext>(descriptor));
            }

            services.AddSingleton(new EntityChangeAttachmentMarker<TContext>());
        }
    }

    private static ServiceDescriptor Wrap<TContext>(ServiceDescriptor original)
        where TContext : DbContext
    {
        return new ServiceDescriptor(
            typeof(DbContextOptions<TContext>),
            sp =>
            {
                var options = ResolveOriginal<TContext>(sp, original);
                var interceptor = sp.GetRequiredService<EntityChangeInterceptor>();

                var core = options.FindExtension<CoreOptionsExtension>();
                if (core?.Interceptors is not null
                    && core.Interceptors.Any(i => ReferenceEquals(i, interceptor)))
                {
                    return options;
                }

                var builder = new DbContextOptionsBuilder<TContext>(options);
                builder.AddInterceptors(interceptor);
                return builder.Options;
            },
            original.Lifetime);
    }

    private static DbContextOptions<TContext> ResolveOriginal<TContext>(
        IServiceProvider sp,
        ServiceDescriptor original)
        where TContext : DbContext
    {
        if (original.ImplementationInstance is DbContextOptions<TContext> instance)
        {
            return instance;
        }

        if (original.ImplementationFactory is not null)
        {
            return (DbContextOptions<TContext>)original.ImplementationFactory(sp);
        }

        if (original.ImplementationType is not null)
        {
            return (DbContextOptions<TContext>)ActivatorUtilities.CreateInstance(sp, original.ImplementationType);
        }

        throw new InvalidOperationException(
            $"Cannot resolve original DbContextOptions<{typeof(TContext).Name}> for EntityChange attachment.");
    }

    private sealed class EntityChangeAttachmentMarker<TContext>
        where TContext : DbContext
    {
    }
}
