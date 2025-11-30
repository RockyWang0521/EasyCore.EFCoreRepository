using EasyCore.MongoDbRepository.EntityBase;

namespace EasyCore.MongoDbRepository.DataFilter
{
    /// <summary>
    /// Interface for data filters.
    /// </summary>
    public interface IDataFilter
    {
        IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> query) where TEntity : class, IEntity;
    }
}
