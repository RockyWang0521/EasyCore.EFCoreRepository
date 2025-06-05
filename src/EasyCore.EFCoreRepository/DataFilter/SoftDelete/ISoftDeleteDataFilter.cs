using EasyCore.EFCoreRepository.Entity;

namespace EasyCore.EFCoreRepository.DataFilter.SoftDelete
{
    /// <summary>
    /// 软删除数据过滤器接口
    /// </summary>
    internal interface ISoftDeleteDataFilter
    {
        IQueryable<TEntity> ApplySoftDeleteDataFilters<TEntity>(IQueryable<TEntity> query) where TEntity : class, IEntity;
    }
}
