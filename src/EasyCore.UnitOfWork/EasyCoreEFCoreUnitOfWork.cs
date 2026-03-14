using Castle.DynamicProxy;
using EasyCore.UnitOfWork.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace EasyCore.UnitOfWork
{
    /// <summary>
    /// Automatically save changes to the database for methods marked with the SaveChangesAttribute.
    /// </summary>
    public static class DataBaseUnitOfWork
    {
        /// <summary>
        /// Registers Unit of Work infrastructure. Assembly scanning is opt-in.
        /// Prefer <see cref="UnitOfWorkBuilder.RegisterSaveChangesFor{TService,TImplementation}"/> for production.
        /// </summary>
        public static UnitOfWorkBuilder EasyCoreUnitOfWork(this IServiceCollection services)
        {
            services.AddScoped<IAsyncInterceptor, SaveChangesStandardInterceptorAsync>();
            services.TryAddSingleton<ProxyGenerator>();
            return new UnitOfWorkBuilder(services);
        }
    }

    public sealed class UnitOfWorkBuilder
    {
        private readonly IServiceCollection _services;

        internal UnitOfWorkBuilder(IServiceCollection services)
        {
            _services = services;
        }

        /// <summary>
        /// Registers a service with SaveChanges proxy when it (or its methods) use <see cref="SaveChangesAttribute"/>.
        /// </summary>
        public UnitOfWorkBuilder RegisterSaveChangesFor<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            _services.AddTransient<TImplementation>();
            _services.AddTransient(typeof(TService), CreateProxyFactory(typeof(TService), typeof(TImplementation)));
            return this;
        }

        /// <summary>
        /// Opt-in: scan application DLLs for types using <see cref="SaveChangesAttribute"/>.
        /// Disabled by default for production safety.
        /// </summary>
        public UnitOfWorkBuilder EnableAssemblyScanning(bool enable = true)
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

            foreach (var dll in dllFiles)
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(dll);
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
                    if (!type.IsClass || type.IsAbstract) continue;

                    bool hasClassAttribute = type.GetCustomAttributes(typeof(SaveChangesAttribute), inherit: false).Any();
                    bool hasMethodAttribute = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Any(m => m.GetCustomAttributes(typeof(SaveChangesAttribute), inherit: false).Any());

                    if (!hasClassAttribute && !hasMethodAttribute) continue;

                    var interfaceType = type.GetInterface("I" + type.Name);
                    if (interfaceType is null) continue;

                    _services.AddTransient(type);
                    _services.AddTransient(interfaceType, CreateProxyFactory(interfaceType, type));
                }
            }

            return this;
        }

        private static Func<IServiceProvider, object> CreateProxyFactory(Type serviceType, Type implementationType)
        {
            return serviceProvider =>
            {
                var generator = serviceProvider.GetRequiredService<ProxyGenerator>();
                var instance = serviceProvider.GetRequiredService(implementationType);
                var interceptors = serviceProvider.GetServices<IAsyncInterceptor>().ToArray();
                return generator.CreateInterfaceProxyWithTarget(serviceType, instance, interceptors);
            };
        }
    }
}
