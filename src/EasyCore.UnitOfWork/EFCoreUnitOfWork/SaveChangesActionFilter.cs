using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyCore.UnitOfWork;

/// <summary>
/// MVC action filter created by <see cref="SaveChangesAttribute"/> via <see cref="IFilterFactory"/>.
/// Persists the configured DbContext after a successful action.
/// </summary>
internal sealed class SaveChangesActionFilter : IAsyncActionFilter
{
    private readonly SaveChangesAttribute _attribute;
    private readonly IServiceProvider _services;
    private readonly ILogger<SaveChangesActionFilter> _logger;

    public SaveChangesActionFilter(
        SaveChangesAttribute attribute,
        IServiceProvider services,
        ILogger<SaveChangesActionFilter> logger)
    {
        _attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        await SaveChangesExecutor.ExecuteAroundActionAsync(
            _attribute,
            _services,
            _logger,
            async () =>
            {
                var executed = await next().ConfigureAwait(false);
                if (executed.Exception is not null && !executed.ExceptionHandled)
                    throw executed.Exception;
            }).ConfigureAwait(false);
    }
}
