using EasyCore.Ambient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace EasyCore.UnitOfWork.Internal;

/// <summary>
/// Sets <see cref="EasyCoreSharedAmbient"/> root at host start.
/// </summary>
internal sealed class EasyCoreAmbientHostedService : IHostedService
{
    private readonly IServiceProvider _root;

    public EasyCoreAmbientHostedService(IServiceProvider root) => _root = root;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        EasyCoreSharedAmbient.SetRoot(_root);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

/// <summary>
/// Ensures HTTP requests publish RequestServices into <see cref="EasyCoreSharedAmbient"/>.
/// </summary>
internal sealed class EasyCoreAmbientStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.Use(async (context, nextMiddleware) =>
            {
                EasyCoreSharedAmbient.SetCurrent(context.RequestServices);
                try
                {
                    await nextMiddleware().ConfigureAwait(false);
                }
                finally
                {
                    EasyCoreSharedAmbient.ClearCurrent();
                }
            });
            next(app);
        };
    }
}

internal static class EasyCoreAmbientRegistration
{
    public static void Register(IServiceCollection services)
    {
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, EasyCoreAmbientHostedService>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IStartupFilter, EasyCoreAmbientStartupFilter>());
    }
}
