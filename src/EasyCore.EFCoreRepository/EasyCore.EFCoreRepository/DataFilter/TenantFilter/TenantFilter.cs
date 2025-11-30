using EasyCore.EFCoreRepository.EntityBase;
using Microsoft.AspNetCore.Http;

namespace EasyCore.EFCoreRepository
{
    /// <summary>
    /// 多租户数据过滤器
    /// </summary>
    internal class TenantFilter : EasyCore.EFCoreRepository.DataFilter.ITenantFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string TenantId => _httpContextAccessor.HttpContext?.Items["TenantId"]?.ToString() ?? string.Empty;

        public TenantFilter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> query) where TEntity : class, IEntity
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
