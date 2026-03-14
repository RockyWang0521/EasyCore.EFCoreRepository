using Castle.DynamicProxy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace EasyCore.UnitOfWork.UnitOfWork
{
    public class SaveChangesStandardInterceptorAsync : AsyncInterceptorBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SaveChangesStandardInterceptorAsync> _logger;

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

            if (attribute == null)
            {
                await proceed.Invoke(invocation, proceedInfo);
                return;
            }

            var dbContext = ResolveDbContext(attribute);

            if (attribute.IsTransaction)
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync();
                try
                {
                    await proceed.Invoke(invocation, proceedInfo);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync();
                    HandleConcurrencyConflict(ex);
                    throw;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            else
            {
                await proceed.Invoke(invocation, proceedInfo);

                try
                {
                    await dbContext.SaveChangesAsync();
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

            if (attribute == null)
                return await proceed.Invoke(invocation, proceedInfo);

            var dbContext = ResolveDbContext(attribute);

            if (attribute.IsTransaction)
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync();
                try
                {
                    var result = await proceed.Invoke(invocation, proceedInfo);
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return result;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync();
                    HandleConcurrencyConflict(ex);
                    throw;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            var ret = await proceed.Invoke(invocation, proceedInfo);

            try
            {
                await dbContext.SaveChangesAsync();
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
            return method.GetCustomAttribute<SaveChangesAttribute>(inherit: true)
                ?? method.DeclaringType?.GetCustomAttribute<SaveChangesAttribute>(inherit: true);
        }

        private DbContext ResolveDbContext(SaveChangesAttribute attribute)
        {
            var dbContext = _serviceProvider.GetService(attribute.DbContextType) as DbContext;
            if (dbContext == null)
                throw new ArgumentException($"DbContext of type '{attribute.DbContextType.FullName}' is not registered.");
            return dbContext;
        }

        private void HandleConcurrencyConflict(DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.Single();
            var databaseValues = entry.GetDatabaseValues();
            var currentValues = entry.OriginalValues;
            var proposedValues = entry.CurrentValues;

            if (databaseValues == null)
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
}
