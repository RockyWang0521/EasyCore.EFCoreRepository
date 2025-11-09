using EasyCore.MongoDbRepository.EntityBase;
using Microsoft.AspNetCore.Http;

namespace EasyCore.MongoDbRepository
{
    /// <summary>
    /// 多租户数据过滤器
    /// </summary>
    internal class TenantFilter : EasyCore.MongoDbRepository.DataFilter.ITenantFilter
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
