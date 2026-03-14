using EasyCore.EFCoreRepository.DataFilter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyCore.EFCoreRepository
{
    public static class EFCoreRepository
    {
        public static void EasyCoreEFCoreRepository(this IServiceCollection services)
        {
            var registeredDbContexts = services.Where(d => typeof(DbContext).IsAssignableFrom(d.ServiceType)).Select(d => d.ServiceType).ToHashSet();

            if (!registeredDbContexts.Any()) return;

            services.AddHttpContextAccessor();

            services.TryAddTransient<ITenantFilter, TenantFilter>();

            services.TryAddTransient<ISoftDeleteFilter, SoftDeleteFilter>();  
        }
    }
}
