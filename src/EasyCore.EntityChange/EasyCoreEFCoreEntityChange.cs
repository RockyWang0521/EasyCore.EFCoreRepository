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
        /// Prefer <see cref="EntityChangeBuilder.AddHandler{THandler}"/> for production handlers.
        /// </summary>
        public static EntityChangeBuilder AddEasyCoreEntityChange(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddOptions<EntityChangeOptions>();
            // Singleton: DbContextOptions typically caches the interceptor instance;
            // handlers are resolved per SaveChanges via IServiceScopeFactory.
            services.TryAddSingleton<EntityChangeInterceptor>();

            EntityChangeDbContextAttachment.AttachAll(services);
            return new EntityChangeBuilder(services);
        }

        /// <summary>
        /// Attaches the entity-change interceptor to every <see cref="DbContextOptions{TContext}"/>
        /// already registered in <paramref name="services"/>. Invoked automatically by repository packages.
        /// </summary>
        public static void AttachEntityChangeToRegisteredDbContexts(IServiceCollection services)
            => EntityChangeDbContextAttachment.AttachAll(services);

        /// <summary>
        /// Explicitly adds the entity-change interceptor to a <see cref="DbContextOptionsBuilder"/>.
        /// Prefer <see cref="AddEasyCoreEntityChange"/> which attaches automatically.
        /// </summary>
        [Obsolete("No longer required. AddEasyCoreEntityChange() attaches the interceptor automatically.")]
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
        /// </summary>
        public EntityChangeBuilder AddHandler<THandler>() where THandler : class
        {
            RegisterHandlerType(typeof(THandler));
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
        /// Opt-in: scan application DLLs for handler implementations.
        /// Disabled by default for production safety.
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
                          || fileName.StartsWith("System.", StringComparison.OrdinalIgnoreCase));
                })
                .ToArray();

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
                    RegisterHandlerType(type);
                }
            }

            EntityChangeDbContextAttachment.AttachAll(_services);
            return this;
        }

        private void RegisterHandlerType(Type type)
        {
            if (!type.IsClass || type.IsAbstract) return;

            foreach (var @interface in type.GetInterfaces().Where(i => i.IsGenericType))
            {
                var genericDef = @interface.GetGenericTypeDefinition();
                if (genericDef == typeof(IEntityAddedChangeHandler<>)
                    || genericDef == typeof(IEntityUpdatedChangeHandler<>)
                    || genericDef == typeof(IEntityDeletedChangeHandler<>))
                {
                    _services.TryAddScoped(@interface, type);
                }
            }
        }
    }
}
