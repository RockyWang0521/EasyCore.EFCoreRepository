using EasyCore.EFCoreRepository.Entity;

namespace EasyCore.EFCoreRepository.DataFilter.Tenant
{
    /// <summary>
    /// 多租户数据过滤器接口
    /// </summary>
    internal interface ITenantDataFilter 
    {
        internal string TenantId { get; }

        IQueryable<TEntity> ApplyTenantDataFilters<TEntity>(IQueryable<TEntity> query) where TEntity : class, IEntity;
    }
}
