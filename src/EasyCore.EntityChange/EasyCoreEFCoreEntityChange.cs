using EasyCore.EntityChange.EntityChange;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace EasyCore.EntityChange
{
    public static class DataBaseEntityChange
    {
        /// <summary>
        /// Registers entity-change infrastructure. The SaveChanges interceptor is attached automatically to
        /// registered <see cref="DbContext"/> instances (call after <c>AddDbContext</c>, or let
        /// <c>AddEasyCoreEFCoreRepository</c> / <c>AddEasyCoreMongoDbRepository</c> attach when used).
        /// By default scans assemblies for handler implementations; set
        /// <paramref name="enableAssemblyScanning"/> to <c>false</c> and use
        /// <see cref="EntityChangeBuilder.AddHandler{THandler}"/> for explicit-only registration.
        /// </summary>
        public static EntityChangeBuilder AddEasyCoreEntityChange(
            this IServiceCollection services,
            bool enableAssemblyScanning = true)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddOptions<EntityChangeOptions>();
            // Singleton: DbContextOptions typically caches the interceptor instance;
            // handlers are resolved per SaveChanges via IServiceScopeFactory.
            services.TryAddSingleton<EntityChangeInterceptor>();

            var builder = new EntityChangeBuilder(services);
            if (enableAssemblyScanning)
            {
                builder.EnableAssemblyScanning();
            }
            else
            {
                EntityChangeDbContextAttachment.AttachAll(services);
            }

            return builder;
        }

        /// <summary>
        /// Attaches the entity-change interceptor to every <see cref="DbContextOptions{TContext}"/>
        /// already registered in <paramref name="services"/>. Invoked automatically by repository packages.
        /// </summary>
        public static void AttachEntityChangeToRegisteredDbContexts(IServiceCollection services)
            => EntityChangeDbContextAttachment.AttachAll(services);

        /// <summary>
        /// Adds the entity-change interceptor to this <see cref="DbContextOptionsBuilder"/>.
        /// Call inside <c>AddDbContext((sp, options) =&gt; options.UseEasyCoreEntityChange(sp))</c>
        /// so each DbContext opts in explicitly (recommended when you have multiple contexts).
        /// </summary>
        public static void UseEasyCoreEntityChange(this DbContextOptionsBuilder builder, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(serviceProvider);

            builder.AddInterceptors(serviceProvider.GetRequiredService<EntityChangeInterceptor>());
        }
    }

    public sealed class EntityChangeBuilder
    {
        private readonly IServiceCollection _services;

        internal EntityChangeBuilder(IServiceCollection services)
        {
            _services = services;
        }

        /// <summary>
        /// Configures entity-change options.
        /// </summary>
        public EntityChangeBuilder Configure(Action<EntityChangeOptions> configure)
        {
            _services.Configure(configure);
            EntityChangeDbContextAttachment.AttachAll(_services);
            return this;
        }

        /// <summary>
        /// Explicitly registers a change handler implementation.
        /// Can be combined with assembly scanning; use this to add handlers that scanning might miss.
        /// </summary>
        public EntityChangeBuilder AddHandler<THandler>() where THandler : class
        {
            RegisterHandlerType(typeof(THandler), replaceExisting: true);
            EntityChangeDbContextAttachment.AttachAll(_services);
            return this;
        }

        /// <summary>
        /// Attaches the interceptor to DbContexts registered so far.
        /// Call after <c>AddDbContext</c> when <see cref="DataBaseEntityChange.AddEasyCoreEntityChange"/>
        /// was invoked before the context registration.
        /// </summary>
        public EntityChangeBuilder AttachToDbContexts()
        {
            EntityChangeDbContextAttachment.AttachAll(_services);
            return this;
        }

        /// <summary>
        /// Scan application DLLs for handler implementations.
        /// Already invoked by default from <see cref="DataBaseEntityChange.AddEasyCoreEntityChange"/>.
        /// </summary>
        public EntityChangeBuilder EnableAssemblyScanning(bool enable = true)
        {
            if (!enable) return this;

            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string[] dllFiles = Directory.GetFiles(rootDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                .Where(path =>
                {
                    string fileName = Path.GetFileName(path);
                    return !(fileName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase)
                          || fileName.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
                          || fileName.StartsWith("Castle.", StringComparison.OrdinalIgnoreCase)
                          || fileName.StartsWith("Newtonsoft.", StringComparison.OrdinalIgnoreCase)
                          || fileName.StartsWith("Swashbuckle.", StringComparison.OrdinalIgnoreCase));
                })
                .ToArray();

            var seen = new HashSet<Type>();

            foreach (string dllFile in dllFiles)
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(dllFile);
                }
                catch
                {
                    continue;
                }

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
                }

                foreach (var type in types)
                {
                    if (!seen.Add(type)) continue;
                    RegisterHandlerType(type, replaceExisting: false);
                }
            }

            EntityChangeDbContextAttachment.AttachAll(_services);
            return this;
        }

        private void RegisterHandlerType(Type type, bool replaceExisting)
        {
            if (!type.IsClass || type.IsAbstract || type.IsGenericTypeDefinition) return;

            var handlerInterfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Where(i =>
                {
                    var def = i.GetGenericTypeDefinition();
                    return def == typeof(IEntityAddedChangeHandler<>)
                        || def == typeof(IEntityUpdatedChangeHandler<>)
                        || def == typeof(IEntityDeletedChangeHandler<>);
                })
                .ToArray();

            if (handlerInterfaces.Length == 0) return;

            _services.TryAddScoped(type);

            foreach (var @interface in handlerInterfaces)
            {
                if (replaceExisting)
                {
                    ReplaceService(@interface, type);
                }
                else
                {
                    _services.TryAddScoped(@interface, sp => sp.GetRequiredService(type));
                }
            }
        }

        private void ReplaceService(Type serviceType, Type implementationType)
        {
            for (var i = _services.Count - 1; i >= 0; i--)
            {
                if (_services[i].ServiceType == serviceType)
                {
                    _services.RemoveAt(i);
                }
            }

            _services.AddScoped(serviceType, sp => sp.GetRequiredService(implementationType));
        }
    }
}
