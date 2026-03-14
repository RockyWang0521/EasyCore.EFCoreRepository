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
        /// Registers entity-change infrastructure. Assembly scanning is opt-in via
        /// <see cref="EntityChangeBuilder.EnableAssemblyScanning"/>.
        /// Prefer <see cref="EntityChangeBuilder.AddHandler{THandler}"/> for production.
        /// </summary>
        public static EntityChangeBuilder EasyCoreEntityChange(this IServiceCollection services)
        {
            services.AddOptions<EntityChangeOptions>();
            services.TryAddScoped<EntityChangeInterceptor>();

            return new EntityChangeBuilder(services);
        }

        /// <summary>
        /// Adds the entity-change interceptor using the application's <see cref="IServiceProvider"/>.
        /// Call from AddDbContext: <c>(sp, opts) => opts.UseEasyCoreEntityChange(sp)</c>.
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
            return this;
        }

        /// <summary>
        /// Explicitly registers a change handler implementation.
        /// </summary>
        public EntityChangeBuilder AddHandler<THandler>() where THandler : class
        {
            RegisterHandlerType(typeof(THandler));
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
