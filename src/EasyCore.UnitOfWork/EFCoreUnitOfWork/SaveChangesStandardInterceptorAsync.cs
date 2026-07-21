using Castle.DynamicProxy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace EasyCore.UnitOfWork.UnitOfWork;

/// <summary>
/// Castle async interceptor that applies <see cref="SaveChangesAttribute"/> resolved from
/// interface / class / method placement.
/// </summary>
public class SaveChangesStandardInterceptorAsync : AsyncInterceptorBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SaveChangesStandardInterceptorAsync> _logger;

    /// <summary>
    /// Pipeline order when multiple <see cref="IAsyncInterceptor"/> are stacked (lower = outer).
    /// Default <c>200</c> — typically inside resilience / cache wrappers.
    /// </summary>
    public int Order { get; set; } = 200;

    public SaveChangesStandardInterceptorAsync(
        IServiceProvider serviceProvider,
        ILogger<SaveChangesStandardInterceptorAsync> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task InterceptAsync(
        IInvocation invocation,
        IInvocationProceedInfo proceedInfo,
        Func<IInvocation, IInvocationProceedInfo, Task> proceed)
    {
        var attribute = ResolveAttribute(invocation);

        if (attribute is null)
        {
            await proceed.Invoke(invocation, proceedInfo).ConfigureAwait(false);
            return;
        }

        var dbContext = ResolveDbContext(attribute);

        if (attribute.IsTransaction)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                await proceed.Invoke(invocation, proceedInfo).ConfigureAwait(false);
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
        }
        else
        {
            await proceed.Invoke(invocation, proceedInfo).ConfigureAwait(false);

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

    protected override async Task<TResult> InterceptAsync<TResult>(
        IInvocation invocation,
        IInvocationProceedInfo proceedInfo,
        Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
    {
        var attribute = ResolveAttribute(invocation);

        if (attribute is null)
            return await proceed.Invoke(invocation, proceedInfo).ConfigureAwait(false);

        var dbContext = ResolveDbContext(attribute);

        if (attribute.IsTransaction)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var result = await proceed.Invoke(invocation, proceedInfo).ConfigureAwait(false);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
                return result;
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
        }

        var ret = await proceed.Invoke(invocation, proceedInfo).ConfigureAwait(false);

        try
        {
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            HandleConcurrencyConflict(ex);
            throw;
        }

        return ret;
    }

    private static SaveChangesAttribute? ResolveAttribute(IInvocation invocation)
    {
        var method = invocation.MethodInvocationTarget ?? invocation.Method;
        var targetType = invocation.TargetType ?? method.DeclaringType ?? typeof(object);
        return SaveChangesAttributeLocator.Find(targetType, method, invocation.Method);
    }

    private DbContext ResolveDbContext(SaveChangesAttribute attribute)
    {
        var dbContext = _serviceProvider.GetService(attribute.DbContextType) as DbContext;
        if (dbContext is null)
            throw new ArgumentException($"DbContext of type '{attribute.DbContextType.FullName}' is not registered.");
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
