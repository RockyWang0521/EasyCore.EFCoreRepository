using EasyCore.EFCoreRepository.DataFilter;
using EasyCore.EFCoreRepository.EntityBase;
using Microsoft.EntityFrameworkCore;

namespace EasyCore.EFCoreRepository
{
    /// <summary>
    /// Multi-tenant data filter.
    /// When current tenant id is available, filters by that value;
    /// when not, filters to rows whose TenantId is null.
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
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                return query.Where(e => EF.Property<string>(e, nameof(IEntityTenant.TenantId)) == null);
            }

            return query.Where(e => EF.Property<string>(e, nameof(IEntityTenant.TenantId)) == tenantId);
        }
    }
}
