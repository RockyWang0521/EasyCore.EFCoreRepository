using EasyCore.EFCoreRepository.EntityBase;

namespace EasyCore.EFCoreRepository
{
    /// <summary>
    /// A filter that filters soft deleted entities from the query.
    /// </summary>
    internal class SoftDeleteFilter : EasyCore.EFCoreRepository.DataFilter.ISoftDeleteFilter
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
