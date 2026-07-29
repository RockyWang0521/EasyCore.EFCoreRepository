using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace EasyCore.UnitOfWork;

/// <summary>
/// Applies Castle proxies for <see cref="SaveChangesAttribute"/>.
/// Nests over existing factory descriptors so multiple packages can stack.
/// </summary>
internal static class SaveChangesCastleProxyApplier
{
    public static void Apply(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<ProxyGenerator>();

        var snapshots = services.ToList();
        foreach (var descriptor in snapshots)
        {
            if (!TryGetImplementationType(snapshots, descriptor, out var impl))
                continue;
            if (impl.IsAbstract || impl.IsInterface)
                continue;
            if (IsLikelyMvcController(impl))
                continue;
            if (!SaveChangesAttributeLocator.IsInstrumented(impl))
                continue;

            // Prefer interface service registrations; skip pure concrete when an interface mapping exists.
            // Exception: Quartz/Hangfire JobWrapper<T> injects concrete T — that descriptor must be proxied
            // so [SaveChanges] on the job type/method still runs when the wrapper calls T.Execute*.
            var serviceType = descriptor.ServiceType;
            if (!serviceType.IsInterface
                && serviceType == impl
                && !JobStyleTypeRules.IsJobStyleImplementation(impl)
                && snapshots.Any(d =>
                    d.ServiceType.IsInterface
                    && d.ServiceType.IsAssignableFrom(impl)
                    && (d.ImplementationType == impl
                        || d.ImplementationFactory is not null
                        || d.ImplementationInstance is not null)))
            {
                continue;
            }

            services.Remove(descriptor);
            var previous = descriptor;
            var lifetime = descriptor.Lifetime;

            services.Add(ServiceDescriptor.Describe(serviceType, sp =>
            {
                var target = CreateTarget(sp, previous, impl);
                var interceptor = new SaveChangesAsyncInterceptor(sp).ToInterceptor();
                var generator = sp.GetRequiredService<ProxyGenerator>();

                if (serviceType.IsInterface)
                    return generator.CreateInterfaceProxyWithTarget(serviceType, target, interceptor);

                var ctorArgs = ResolveConstructorArguments(sp, impl);
                return generator.CreateClassProxyWithTarget(impl, target, ctorArgs, interceptor);
            }, lifetime));
        }
    }

    private static bool TryGetImplementationType(
        List<ServiceDescriptor> snapshots,
        ServiceDescriptor descriptor,
        out Type impl)
    {
        if (descriptor.ImplementationType is not null)
        {
            impl = descriptor.ImplementationType;
            return true;
        }

        if (descriptor.ImplementationFactory is null && descriptor.ImplementationInstance is null)
        {
            impl = null!;
            return false;
        }

        impl = FindInstrumentedImpl(snapshots, descriptor.ServiceType)!;
        return impl is not null;
    }

    private static Type? FindInstrumentedImpl(List<ServiceDescriptor> snapshots, Type serviceType)
    {
        foreach (var d in snapshots)
        {
            if (d.ImplementationType is null)
                continue;
            var t = d.ImplementationType;
            if (t.IsAbstract || t.IsInterface)
                continue;
            if (!serviceType.IsAssignableFrom(t))
                continue;
            if (SaveChangesAttributeLocator.IsInstrumented(t))
                return t;
        }

        return null;
    }

    private static object CreateTarget(IServiceProvider sp, ServiceDescriptor previous, Type impl)
    {
        if (previous.ImplementationFactory is not null)
            return previous.ImplementationFactory(sp)!;
        if (previous.ImplementationInstance is not null)
            return previous.ImplementationInstance;
        return ActivatorUtilities.CreateInstance(sp, impl);
    }

    private static object[] ResolveConstructorArguments(IServiceProvider sp, Type implementationType)
    {
        var ctors = implementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
        if (ctors.Length == 0)
            return Array.Empty<object>();

        var ctor = ctors.OrderByDescending(c => c.GetParameters().Length).First();
        var parameters = ctor.GetParameters();
        var args = new object[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
            args[i] = ActivatorUtilities.GetServiceOrCreateInstance(sp, parameters[i].ParameterType);
        return args;
    }

    private static bool IsLikelyMvcController(Type type)
    {
        for (var t = type; t is not null; t = t.BaseType)
        {
            if (t.Name is "ControllerBase" or "Controller")
                return true;
        }

        return false;
    }
}
