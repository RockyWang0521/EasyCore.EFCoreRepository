using EasyCore.EFCoreRepository.DataFilter;
using EasyCore.EFCoreRepository.EntityBase;
using Microsoft.EntityFrameworkCore;

namespace EasyCore.EFCoreRepository
{
    /// <summary>
    /// A filter that excludes soft-deleted entities from queries.
    /// </summary>
    internal class SoftDeleteFilter : ISoftDeleteFilter
    {
        public IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> query) where TEntity : class, IEntity
        {
            if (typeof(IEntitySoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !EF.Property<bool>(e, nameof(IEntitySoftDelete.IsDeleted)));
            }

            return query;
        }
    }
}
