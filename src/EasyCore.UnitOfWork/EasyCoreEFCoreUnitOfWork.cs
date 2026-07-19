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
        /// Registers Unit of Work infrastructure.
        /// By default scans application assemblies and wraps types that use <see cref="SaveChangesAttribute"/> with proxies.
        /// Use <paramref name="enableAssemblyScanning"/> = <c>false</c> plus
        /// <see cref="UnitOfWorkBuilder.RegisterSaveChangesFor{TService,TImplementation}"/> for explicit-only registration.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="enableAssemblyScanning">
        /// When <c>true</c> (default), auto-registers all concrete types that carry
        /// <see cref="SaveChangesAttribute"/> on the class or any instance method.
        /// </param>
        public static UnitOfWorkBuilder AddEasyCoreUnitOfWork(
            this IServiceCollection services,
            bool enableAssemblyScanning = true)
        {
            services.AddScoped<IAsyncInterceptor, SaveChangesStandardInterceptorAsync>();
            services.TryAddSingleton<ProxyGenerator>();

            var builder = new UnitOfWorkBuilder(services);
            if (enableAssemblyScanning)
            {
                builder.EnableAssemblyScanning();
            }

            return builder;
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
        /// Explicitly registers a service with a SaveChanges proxy.
        /// Can be combined with assembly scanning; explicit registration wins for the same interface.
        /// </summary>
        public UnitOfWorkBuilder RegisterSaveChangesFor<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            RegisterProxy(typeof(TService), typeof(TImplementation));
            return this;
        }

        /// <summary>
        /// Scan application DLLs for types using <see cref="SaveChangesAttribute"/> and register proxies.
        /// Already invoked by default from <see cref="DataBaseUnitOfWork.AddEasyCoreUnitOfWork"/>.
        /// Call again only if you need a re-scan after loading more assemblies.
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
                          || fileName.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
                          || fileName.StartsWith("Castle.", StringComparison.OrdinalIgnoreCase)
                          || fileName.StartsWith("Newtonsoft.", StringComparison.OrdinalIgnoreCase)
                          || fileName.StartsWith("Swashbuckle.", StringComparison.OrdinalIgnoreCase));
                })
                .ToArray();

            var registered = new HashSet<Type>();

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
                    if (!type.IsClass || type.IsAbstract || type.IsGenericTypeDefinition) continue;
                    if (!registered.Add(type)) continue;

                    bool hasClassAttribute = type.GetCustomAttributes(typeof(SaveChangesAttribute), inherit: false).Any();
                    bool hasMethodAttribute = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Any(m => m.GetCustomAttributes(typeof(SaveChangesAttribute), inherit: false).Any());

                    if (!hasClassAttribute && !hasMethodAttribute) continue;

                    var interfaceType = ResolveServiceInterface(type);
                    if (interfaceType is null) continue;

                    RegisterProxy(interfaceType, type);
                }
            }

            return this;
        }

        private void RegisterProxy(Type serviceType, Type implementationType)
        {
            _services.TryAddTransient(implementationType);
            ReplaceService(serviceType, CreateProxyFactory(serviceType, implementationType));
        }

        private void ReplaceService(Type serviceType, Func<IServiceProvider, object> factory)
        {
            for (var i = _services.Count - 1; i >= 0; i--)
            {
                if (_services[i].ServiceType == serviceType)
                {
                    _services.RemoveAt(i);
                }
            }

            _services.AddTransient(serviceType, factory);
        }

        /// <summary>
        /// Prefers <c>I{ClassName}</c>; otherwise the single non-marker interface on the type.
        /// </summary>
        private static Type? ResolveServiceInterface(Type implementationType)
        {
            var byConvention = implementationType.GetInterface("I" + implementationType.Name);
            if (byConvention != null) return byConvention;

            var candidates = implementationType.GetInterfaces()
                .Where(i => i != typeof(IDisposable)
                         && i != typeof(IAsyncDisposable)
                         && (i.Namespace == null || !i.Namespace.StartsWith("System", StringComparison.Ordinal))
                         && !i.Name.EndsWith("Dependencie", StringComparison.Ordinal)
                         && !i.Name.EndsWith("Dependency", StringComparison.Ordinal))
                .ToArray();

            return candidates.Length == 1 ? candidates[0] : null;
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
