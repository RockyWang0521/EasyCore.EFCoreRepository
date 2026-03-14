using EasyCore.MongoDbRepository.EntityBase;

namespace EasyCore.MongoDbRepository.DataFilter
{
    /// <summary>
    /// Applies a filter to an entity query.
    /// </summary>
    public interface IDataFilter
    {
        IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> query) where TEntity : class, IEntity;
    }

    /// <summary>
    /// Soft-delete query filter.
    /// </summary>
    public interface ISoftDeleteFilter : IDataFilter
    {
    }

    /// <summary>
    /// Tenant query filter.
    /// </summary>
    public interface ITenantFilter : IDataFilter
    {
        string TenantId { get; }
    }

    /// <summary>
    /// Provides the current tenant identifier.
    /// </summary>
    public interface ITenantProvider
    {
        string? GetTenantId();
    }
}
