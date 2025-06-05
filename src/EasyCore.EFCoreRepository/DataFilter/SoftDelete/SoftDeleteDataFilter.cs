using EasyCore.EFCoreRepository.EntityBase;

namespace EasyCore.EFCoreRepository.DataFilter.SoftDelete
{
    /// <summary>
    /// 软删除数据过滤器
    /// </summary>
    internal class SoftDeleteDataFilter : ISoftDeleteDataFilter
    {
        IQueryable<TEntity> ISoftDeleteDataFilter.ApplySoftDeleteDataFilters<TEntity>(IQueryable<TEntity> query)
        {
            if (typeof(IEntitySoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => ((IEntitySoftDelete)e!).IsDeleted == false);
            }
            return query;
        }
    }
}
