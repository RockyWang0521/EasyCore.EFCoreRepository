using EasyCore.UnitOfWork.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace EasyCore.UnitOfWork;

/// <summary>
/// Automatically save changes for methods marked with <see cref="SaveChangesAttribute"/>.
/// Compile-time AspectInjector weave for services / event handlers; MVC Filter for Controllers / Dynamic API.
/// </summary>
public static class DataBaseUnitOfWork
{
    /// <summary>
    /// Registers Unit of Work infrastructure (ambient DI scope + MVC convention).
    /// Assembly scanning / Castle proxies are no longer required; weaving happens at compile time.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="enableAssemblyScanning">Ignored; retained for API compatibility.</param>
    public static UnitOfWorkBuilder AddEasyCoreUnitOfWork(
        this IServiceCollection services,
        bool enableAssemblyScanning = true)
    {
        ArgumentNullException.ThrowIfNull(services);

        EasyCoreAmbientRegistration.Register(services);
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IConfigureOptions<MvcOptions>, SaveChangesMvcOptionsSetup>());

        return new UnitOfWorkBuilder(services);
    }
}

/// <summary>
/// Fluent registration helpers retained for API compatibility. Proxy registration is a no-op after weaving.
/// </summary>
public sealed class UnitOfWorkBuilder
{
    private readonly IServiceCollection _services;

    internal UnitOfWorkBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Obsolete: AspectInjector weaves <see cref="SaveChangesAttribute"/> at compile time; no proxy registration needed.
    /// </summary>
    [Obsolete("SaveChanges is applied via AspectInjector compile-time weaving. Explicit proxy registration is no longer required.")]
    public UnitOfWorkBuilder RegisterSaveChangesFor<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
        => this;

    /// <summary>
    /// Obsolete: assembly scanning for Castle proxies has been removed.
    /// </summary>
    [Obsolete("SaveChanges is applied via AspectInjector compile-time weaving. Assembly scanning is no longer required.")]
    public UnitOfWorkBuilder EnableAssemblyScanning(bool enable = true) => this;
}
