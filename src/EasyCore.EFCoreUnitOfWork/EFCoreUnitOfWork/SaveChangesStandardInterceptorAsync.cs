using Castle.DynamicProxy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace EasyCore.EFCoreUnitOfWork.EFCoreUnitOfWork
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

        protected async override Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task> proceed)
        {
            var method = invocation.MethodInvocationTarget ?? invocation.Method;

            var attribute = method.GetCustomAttribute<SaveChangesAttribute>(inherit: true);

            if (attribute == null) attribute = method.DeclaringType?.GetCustomAttribute<SaveChangesAttribute>(inherit: true);

            if (attribute == null)
            {
                await proceed.Invoke(invocation, proceedInfo);

                return;
            }

            var _dbContext = _serviceProvider.GetService(attribute.DbContextType) as DbContext;

            if (_dbContext == null) throw new ArgumentException("DbContext is null");

            if (attribute.IsTransaction)
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        await proceed.Invoke(invocation, proceedInfo);

                        _dbContext.SaveChanges();

                        transaction.Commit();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        transaction.Rollback();

                        HandleConcurrencyConflict(ex);

                        throw;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();

                        throw;
                    }
                }
            }
            else
            {
                await proceed.Invoke(invocation, proceedInfo);

                try { _dbContext.SaveChanges(); }
                catch (DbUpdateConcurrencyException ex)
                {
                    HandleConcurrencyConflict(ex);
                    throw;
                }
                catch (Exception) { throw; }
            }
        }

        protected async override Task<TResult> InterceptAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
        {
            TResult? ret = default;

            var method = invocation.MethodInvocationTarget ?? invocation.Method;

            var attribute = method.GetCustomAttribute<SaveChangesAttribute>(inherit: true);

            if (attribute == null) attribute = method.DeclaringType?.GetCustomAttribute<SaveChangesAttribute>(inherit: true);

            if (attribute == null) return (TResult)await proceed.Invoke(invocation, proceedInfo);

            var _dbContext = _serviceProvider.GetService(attribute.DbContextType) as DbContext;

            if (_dbContext == null) throw new ArgumentException("DbContext is null");

            if (attribute.IsTransaction)
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        ret = await proceed.Invoke(invocation, proceedInfo);

                        _dbContext.SaveChanges();

                        transaction.Commit();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        transaction.Rollback();

                        HandleConcurrencyConflict(ex);

                        throw;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();

                        throw;
                    }
                }
            }
            else
            {
                ret = await proceed.Invoke(invocation, proceedInfo);

                try { _dbContext.SaveChanges(); }
                catch (DbUpdateConcurrencyException ex)
                {
                    HandleConcurrencyConflict(ex);

                    throw;
                }
                catch (Exception) { throw; }
            }
            return (TResult)ret!;
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

            _logger.LogError("Concurrency conflicts occur : " + conflictDetails);
        }
    }
}
