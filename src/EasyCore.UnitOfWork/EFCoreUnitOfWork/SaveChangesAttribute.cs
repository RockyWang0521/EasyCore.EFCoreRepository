using AspectInjector.Broker;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.UnitOfWork;

/// <summary>
/// Marks a class, interface, method, or MVC controller / action to persist EF Core changes after the call.
/// Non-controller methods are woven at compile time via AspectInjector; Dynamic API / Controllers use
/// <see cref="IFilterFactory"/> (aspect no-ops on <c>ControllerBase</c> to avoid double save).
/// </summary>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface,
    Inherited = true,
    AllowMultiple = false)]
[Injection(typeof(SaveChangesAspect))]
public sealed class SaveChangesAttribute : Attribute, IFilterFactory, IOrderedFilter
{
    /// <summary>
    /// MVC / aspect stack order. Lower = outer. Default: <c>200</c> (typically inside resilience / cache).
    /// </summary>
    public int Order { get; set; } = 200;

    /// <summary>
    /// When <c>true</c>, wraps the call in an explicit database transaction.
    /// </summary>
    public bool IsTransaction { get; set; }

    /// <summary>
    /// Concrete <see cref="DbContext"/> type resolved from DI.
    /// </summary>
    public Type DbContextType { get; }

    /// <summary>
    /// Creates a SaveChanges marker for the given <see cref="DbContext"/> type.
    /// </summary>
    public SaveChangesAttribute(Type dbContextType)
    {
        if (dbContextType is null || !typeof(DbContext).IsAssignableFrom(dbContextType))
            throw new ArgumentException("Provided type must be a DbContext.", nameof(dbContextType));

        DbContextType = dbContextType;
    }

    /// <summary>
    /// Creates a SaveChanges marker with an optional explicit transaction.
    /// </summary>
    public SaveChangesAttribute(bool isTransaction, Type dbContextType)
        : this(dbContextType)
    {
        IsTransaction = isTransaction;
    }

    /// <inheritdoc />
    bool IFilterFactory.IsReusable => false;

    /// <inheritdoc />
    int IOrderedFilter.Order => Order;

    /// <inheritdoc />
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        return ActivatorUtilities.CreateInstance<SaveChangesActionFilter>(serviceProvider, this);
    }
}
