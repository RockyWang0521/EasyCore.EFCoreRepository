using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyCore.UnitOfWork;

/// <summary>
/// MVC action filter created by <see cref="SaveChangesAttribute"/> via <see cref="IFilterFactory"/>.
/// Persists the configured <see cref="DbContext"/> after a successful action.
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

        var dbContext = ResolveDbContext();

        if (_attribute.IsTransaction)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var executed = await next().ConfigureAwait(false);
                if (executed.Exception is not null && !executed.ExceptionHandled)
                    throw executed.Exception;

                await dbContext.SaveChangesAsync().ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                HandleConcurrencyConflict(ex);
                throw;
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }

            return;
        }

        {
            var executed = await next().ConfigureAwait(false);
            if (executed.Exception is not null && !executed.ExceptionHandled)
                return;

            try
            {
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                HandleConcurrencyConflict(ex);
                throw;
            }
        }
    }

    private DbContext ResolveDbContext()
    {
        var dbContext = _services.GetService(_attribute.DbContextType) as DbContext;
        if (dbContext is null)
            throw new ArgumentException($"DbContext of type '{_attribute.DbContextType.FullName}' is not registered.");
        return dbContext;
    }

    private void HandleConcurrencyConflict(DbUpdateConcurrencyException ex)
    {
        var entry = ex.Entries.Single();
        var databaseValues = entry.GetDatabaseValues();
        var currentValues = entry.OriginalValues;
        var proposedValues = entry.CurrentValues;

        if (databaseValues is null)
            _logger.LogError("The record has been deleted by another user.");
        else
            LogConcurrencyConflict(proposedValues, currentValues);
    }

    private void LogConcurrencyConflict(PropertyValues proposedValues, PropertyValues currentValues)
    {
        var conflictDetails = string.Join(", ", proposedValues.EntityType.GetProperties().Select(property =>
        {
            var fieldName = property.Name;
            var proposedValue = proposedValues[fieldName];
            var currentValue = currentValues[fieldName];
            return $"{fieldName}: Proposed='{proposedValue}', Current='{currentValue}'";
        }));

        _logger.LogError("Concurrency conflicts occur : {ConflictDetails}", conflictDetails);
    }
}
