using EasyCore.MongoDbRepository.EntityBase;

namespace EasyCore.MongoDbRepository
{
    /// <summary>
    /// A filter that filters soft deleted entities from the query.
    /// </summary>
    internal class SoftDeleteFilter : EasyCore.MongoDbRepository.DataFilter.ISoftDeleteFilter
    {
        public IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> query) where TEntity : class, IEntity
        {
            if (typeof(IEntitySoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => ((IEntitySoftDelete)e!).IsDeleted == false);
            }
            return query;
        }
    }
}
