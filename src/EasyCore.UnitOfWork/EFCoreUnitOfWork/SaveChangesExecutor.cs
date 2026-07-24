using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;

namespace EasyCore.UnitOfWork;

/// <summary>
/// Executes SaveChanges / optional transaction around a method body.
/// Shared by Castle <see cref="SaveChangesAsyncInterceptor"/> and MVC <see cref="SaveChangesActionFilter"/>.
/// </summary>
internal static class SaveChangesExecutor
{
    public static object? Execute(
        object? instance,
        MethodBase method,
        object[] args,
        Func<object[], object> target,
        Type returnType,
        SaveChangesAttribute attribute,
        IServiceProvider? services = null)
    {
        if (instance is ControllerBase || IsControllerDeclaringType(method))
            return target(args);

        if (typeof(Task).IsAssignableFrom(returnType))
        {
            var resultType = returnType.IsGenericType
                ? returnType.GenericTypeArguments[0]
                : typeof(object);
            return typeof(SaveChangesExecutor)
                .GetMethod(nameof(ExecuteAsyncTyped), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(resultType)
                .Invoke(null, new object?[] { target, args, attribute, services })!;
        }

        if (returnType == typeof(ValueTask)
            || (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>)))
        {
            var resultType = returnType.IsGenericType
                ? returnType.GenericTypeArguments[0]
                : typeof(object);
            var task = typeof(SaveChangesExecutor)
                .GetMethod(nameof(ExecuteAsyncTyped), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(resultType)
                .Invoke(null, new object?[] { target, args, attribute, services })!;

            if (!returnType.IsGenericType)
                return new ValueTask((Task)task);

            return typeof(SaveChangesExecutor)
                .GetMethod(nameof(ToValueTask), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(resultType)
                .Invoke(null, new[] { task })!;
        }

        return ExecuteSync(target, args, attribute, services);
    }

    private static bool IsControllerDeclaringType(MethodBase method)
    {
        var declaring = method.DeclaringType;
        return declaring is not null && typeof(ControllerBase).IsAssignableFrom(declaring);
    }

    private static object? ExecuteSync(
        Func<object[], object> target,
        object[] args,
        SaveChangesAttribute attribute,
        IServiceProvider? services)
    {
        var sp = RequireServices(services);
        var dbContext = ResolveDbContext(sp, attribute);
        var logger = sp.GetService<ILoggerFactory>()?.CreateLogger("EasyCore.UnitOfWork.SaveChanges")
                     ?? NullLogger.Instance;

        if (attribute.IsTransaction)
        {
            using var transaction = dbContext.Database.BeginTransaction();
            try
            {
                var result = target(args);
                dbContext.SaveChanges();
                transaction.Commit();
                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                transaction.Rollback();
                HandleConcurrencyConflict(ex, logger);
                throw;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        var ret = target(args);
        try
        {
            dbContext.SaveChanges();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            HandleConcurrencyConflict(ex, logger);
            throw;
        }

        return ret;
    }

    private static async Task<T> ExecuteAsyncTyped<T>(
        Func<object[], object> target,
        object[] args,
        SaveChangesAttribute attribute,
        IServiceProvider? services)
    {
        var sp = RequireServices(services);
        var dbContext = ResolveDbContext(sp, attribute);
        var logger = sp.GetService<ILoggerFactory>()?.CreateLogger("EasyCore.UnitOfWork.SaveChanges")
                     ?? NullLogger.Instance;

        if (attribute.IsTransaction)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var result = await InvokeAsync<T>(target, args).ConfigureAwait(false);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                HandleConcurrencyConflict(ex, logger);
                throw;
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        var ret = await InvokeAsync<T>(target, args).ConfigureAwait(false);
        try
        {
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            HandleConcurrencyConflict(ex, logger);
            throw;
        }

        return ret;
    }

    private static async Task<T> InvokeAsync<T>(Func<object[], object> target, object[] args)
    {
        var invoked = target(args);
        if (invoked is Task<T> typed)
            return await typed.ConfigureAwait(false);

        if (invoked is ValueTask<T> valueTaskTyped)
            return await valueTaskTyped.ConfigureAwait(false);

        if (invoked is ValueTask valueTask)
        {
            await valueTask.ConfigureAwait(false);
            return default!;
        }

        if (invoked is Task task)
        {
            await task.ConfigureAwait(false);
            return default!;
        }

        return invoked is T direct ? direct : default!;
    }

    private static ValueTask<T> ToValueTask<T>(object taskObj) => new((Task<T>)taskObj);

    public static async Task ExecuteAroundActionAsync(
        SaveChangesAttribute attribute,
        IServiceProvider services,
        ILogger logger,
        Func<Task> proceed)
    {
        var dbContext = ResolveDbContext(services, attribute);

        if (attribute.IsTransaction)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                await proceed().ConfigureAwait(false);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                HandleConcurrencyConflict(ex, logger);
                throw;
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }

            return;
        }

        await proceed().ConfigureAwait(false);
        try
        {
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            HandleConcurrencyConflict(ex, logger);
            throw;
        }
    }

    private static IServiceProvider RequireServices(IServiceProvider? services)
    {
        if (services is not null)
            return services;

        throw new InvalidOperationException(
            "IServiceProvider was not provided. Castle SaveChangesAsyncInterceptor must pass the DI provider; Ambient is not supported.");
    }

    private static DbContext ResolveDbContext(IServiceProvider services, SaveChangesAttribute attribute)
    {
        var dbContext = services.GetService(attribute.DbContextType) as DbContext;
        if (dbContext is null)
            throw new ArgumentException($"DbContext of type '{attribute.DbContextType.FullName}' is not registered.");
        return dbContext;
    }

    private static void HandleConcurrencyConflict(DbUpdateConcurrencyException ex, ILogger logger)
    {
        var entry = ex.Entries.Single();
        var databaseValues = entry.GetDatabaseValues();
        var currentValues = entry.OriginalValues;
        var proposedValues = entry.CurrentValues;

        if (databaseValues is null)
            logger.LogError("The record has been deleted by another user.");
        else
            LogConcurrencyConflict(proposedValues, currentValues, logger);
    }

    private static void LogConcurrencyConflict(PropertyValues proposedValues, PropertyValues currentValues, ILogger logger)
    {
        var conflictDetails = string.Join(", ", proposedValues.EntityType.GetProperties().Select(property =>
        {
            var fieldName = property.Name;
            var proposedValue = proposedValues[fieldName];
            var currentValue = currentValues[fieldName];
            return $"{fieldName}: Proposed='{proposedValue}', Current='{currentValue}'";
        }));

        logger.LogError("Concurrency conflicts occur : {ConflictDetails}", conflictDetails);
    }
}
