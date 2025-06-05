using EasyCore.EFCoreRepository.EntityBase;
using Microsoft.AspNetCore.Http;

namespace EasyCore.EFCoreRepository.DataFilter.TenantFilter
{
    /// <summary>
    /// 多租户数据过滤器
    /// </summary>
    internal class TenantDataFilter : ITenantDataFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string TenantId => _httpContextAccessor.HttpContext?.Items["TenantId"]?.ToString() ?? string.Empty;

        public TenantDataFilter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IQueryable<TEntity> ApplyTenantDataFilters<TEntity>(IQueryable<TEntity> query) where TEntity : class, IEntity
        {
            if (typeof(IEntityTenant).IsAssignableFrom(typeof(TEntity)))
            {
                var tenantId = TenantId;
                if (string.IsNullOrEmpty(tenantId)) return query;
                query = query.Where(e => ((IEntityTenant)e!).TenantId == tenantId);
            }
            return query;
        }
    }
}
