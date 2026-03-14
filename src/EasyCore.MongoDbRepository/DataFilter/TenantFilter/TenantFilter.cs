using EasyCore.MongoDbRepository.DataFilter;
using EasyCore.MongoDbRepository.EntityBase;
using Microsoft.EntityFrameworkCore;

namespace EasyCore.MongoDbRepository
{
    /// <summary>
    /// Multi-tenant data filter. Fail-closed when tenant id is unavailable.
    /// </summary>
    internal class TenantFilter : ITenantFilter
    {
        private readonly ITenantProvider _tenantProvider;

        public string TenantId => _tenantProvider.GetTenantId() ?? string.Empty;

        public TenantFilter(ITenantProvider tenantProvider)
        {
            _tenantProvider = tenantProvider;
        }

        public IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> query) where TEntity : class, IEntity
        {
            if (!typeof(IEntityTenant).IsAssignableFrom(typeof(TEntity)))
                return query;

            var tenantId = TenantId;
            if (string.IsNullOrEmpty(tenantId))
                return query.Where(_ => false);

            return query.Where(e => EF.Property<string>(e, nameof(IEntityTenant.TenantId)) == tenantId);
        }
    }
}
