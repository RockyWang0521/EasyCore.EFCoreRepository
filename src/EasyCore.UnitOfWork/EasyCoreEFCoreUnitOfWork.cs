using System.Reflection;
using Castle.DynamicProxy;
using EasyCore.UnitOfWork.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace EasyCore.UnitOfWork;

/// <summary>
/// Automatically save changes for methods marked with <see cref="SaveChangesAttribute"/>.
/// Dual path: Castle <see cref="IAsyncInterceptor"/> (services) + MVC Filter (Dynamic API / Controllers).
/// </summary>
public static class DataBaseUnitOfWork
{
    /// <summary>
    /// Registers Unit of Work infrastructure (interceptor + MVC convention).
    /// By default scans application assemblies and wraps non-controller types that use
    /// <see cref="SaveChangesAttribute"/> with proxies.
    /// Use <paramref name="enableAssemblyScanning"/> = <c>false</c> plus
    /// <see cref="UnitOfWorkBuilder.RegisterSaveChangesFor{TService,TImplementation}"/> for explicit-only registration.
    /// </summary>
    public static UnitOfWorkBuilder AddEasyCoreUnitOfWork(
        this IServiceCollection services,
        bool enableAssemblyScanning = true)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<ProxyGenerator>();
        services.TryAddScoped<SaveChangesStandardInterceptorAsync>();
        services.TryAddEnumerable(
            ServiceDescriptor.Scoped<IAsyncInterceptor, SaveChangesStandardInterceptorAsync>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigureOptions<MvcOptions>, SaveChangesMvcOptionsSetup>());

        var builder = new UnitOfWorkBuilder(services);
        if (enableAssemblyScanning)
            builder.EnableAssemblyScanning();

        return builder;
    }
}

/// <summary>
/// Fluent registration helpers for SaveChanges proxies.
/// </summary>
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
    /// Skips <see cref="ControllerBase"/> (including EasyCoreAppService) — those use the MVC Filter path.
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
                if (typeof(ControllerBase).IsAssignableFrom(type)) continue;
                if (!registered.Add(type)) continue;
                if (!SaveChangesAttributeLocator.IsInstrumented(type)) continue;

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
                _services.RemoveAt(i);
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
            var interceptors = OrderInterceptors(serviceProvider.GetServices<IAsyncInterceptor>());
            return generator.CreateInterfaceProxyWithTarget(serviceType, instance, interceptors);
        };
    }

    private static IAsyncInterceptor[] OrderInterceptors(IEnumerable<IAsyncInterceptor> interceptors)
        => interceptors
            .OrderBy(GetInterceptorOrder)
            .ThenBy(i => i.GetType().FullName, StringComparer.Ordinal)
            .ToArray();

    private static int GetInterceptorOrder(IAsyncInterceptor interceptor)
    {
        var prop = interceptor.GetType().GetProperty(
            "Order",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop?.PropertyType == typeof(int) && prop.CanRead)
            return (int)prop.GetValue(interceptor)!;
        return 0;
    }
}
