using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace EasyCore.UnitOfWork;

/// <summary>
/// Automatically save changes for methods marked with <see cref="SaveChangesAttribute"/>.
/// Castle DynamicProxy for services / event handlers; MVC Filter for Controllers / Dynamic API.
/// </summary>
public static class DataBaseUnitOfWork
{
    /// <summary>
    /// Registers Unit of Work infrastructure (Castle DynamicProxy + MVC convention).
    /// When <paramref name="enableAssemblyScanning"/> is true, discovers types with
    /// <see cref="SaveChangesAttribute"/> and registers them before applying proxies.
    /// Prefer registering your services first, then calling this so existing descriptors are wrapped.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="enableAssemblyScanning">When true, scans loaded assemblies for instrumented types.</param>
    public static UnitOfWorkBuilder AddEasyCoreUnitOfWork(
        this IServiceCollection services,
        bool enableAssemblyScanning = true)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigureOptions<MvcOptions>, SaveChangesMvcOptionsSetup>());

        if (enableAssemblyScanning)
        {
            RegisterDiscoveredServices(services);
        }

        SaveChangesCastleProxyApplier.Apply(services);
        return new UnitOfWorkBuilder(services);
    }

    private static void RegisterDiscoveredServices(IServiceCollection services)
    {
        var types = GetAutoScanAssemblies()
            .SelectMany(SafeGetTypes)
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => !typeof(ControllerBase).IsAssignableFrom(t))
            .Where(SaveChangesAttributeLocator.IsInstrumented)
            .Distinct()
            .ToList();

        foreach (var implementation in types)
        {
            var interfaces = FindRegisterableInterfaces(implementation);
            services.TryAddTransient(implementation);
            foreach (var interfaceType in interfaces)
            {
                services.TryAddTransient(interfaceType, implementation);
            }
        }
    }

    private static IReadOnlyList<Type> FindRegisterableInterfaces(Type implementation)
    {
        return implementation.GetInterfaces()
            .Where(i => !IsFrameworkInterface(i))
            .Distinct()
            .ToList();
    }

    private static bool IsFrameworkInterface(Type type)
    {
        if (type.IsGenericType && !type.IsGenericTypeDefinition)
            type = type.GetGenericTypeDefinition();

        var ns = type.Namespace ?? string.Empty;
        if (ns.StartsWith("System", StringComparison.Ordinal)
            || ns.StartsWith("Microsoft", StringComparison.Ordinal)
            || ns.StartsWith("Castle", StringComparison.Ordinal))
        {
            return true;
        }

        // Quartz / Hangfire job marker interfaces.
        // EasyCore.Quartz / EasyCore.Hangfire JobWrapper<T> resolve the concrete job type T from DI,
        // so these interfaces must not become the preferred Castle proxy service registration.
        if (JobStyleTypeRules.IsJobStyleInterfaceName(type.Name))
        {
            return true;
        }

        return false;
    }

    private static IEnumerable<Assembly> GetAutoScanAssemblies()
    {
        var result = new HashSet<Assembly>();

        void TryAdd(Assembly? assembly)
        {
            if (assembly is null || assembly.IsDynamic)
                return;
            if (IsFrameworkOrInfrastructure(assembly))
                return;
            result.Add(assembly);
        }

        var entry = Assembly.GetEntryAssembly();
        TryAdd(entry);

        if (entry is not null)
        {
            foreach (var reference in entry.GetReferencedAssemblies())
            {
                try
                {
                    TryAdd(Assembly.Load(reference));
                }
                catch (Exception)
                {
                    // Ignore unloadable references.
                }
            }
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            TryAdd(assembly);

        return result;
    }

    private static bool IsFrameworkOrInfrastructure(Assembly assembly)
    {
        var name = assembly.GetName().Name ?? string.Empty;
        return name.StartsWith("System", StringComparison.OrdinalIgnoreCase)
               || name.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase)
               || name.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase)
               || name.StartsWith("Castle.", StringComparison.OrdinalIgnoreCase)
               || name.StartsWith("EasyCore.UnitOfWork", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }
}

/// <summary>
/// Fluent registration helpers for Castle DynamicProxy wrapping of SaveChanges services.
/// </summary>
public sealed class UnitOfWorkBuilder
{
    private readonly IServiceCollection _services;

    internal UnitOfWorkBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Registers an interface/implementation pair and re-applies Castle SaveChanges proxies.
    /// </summary>
    public UnitOfWorkBuilder RegisterSaveChangesFor<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        _services.TryAddTransient<TImplementation>();
        _services.TryAddTransient<TService, TImplementation>();
        SaveChangesCastleProxyApplier.Apply(_services);
        return this;
    }

    /// <summary>
    /// Obsolete: assembly scanning is controlled by <see cref="DataBaseUnitOfWork.AddEasyCoreUnitOfWork"/>;
    /// this method is retained for API compatibility and is a no-op.
    /// </summary>
    [Obsolete("Pass enableAssemblyScanning to AddEasyCoreUnitOfWork instead. Castle proxies are applied there.")]
    public UnitOfWorkBuilder EnableAssemblyScanning(bool enable = true) => this;
}
