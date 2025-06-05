using EasyCore.EFCoreRepository.DataFilter.SoftDelete;
using EasyCore.EFCoreRepository.DataFilter.Tenant;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.EFCoreRepository.Repository
{
    public class BaseEfCoreRepository
    {
        internal readonly IServiceProvider ServiceProvider;

        internal readonly ITenantDataFilter TenantDataFilter;

        internal readonly ISoftDeleteDataFilter SoftDeleteDataFilter;

        public BaseEfCoreRepository(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            TenantDataFilter = ServiceProvider!.GetService<ITenantDataFilter>()!;
            SoftDeleteDataFilter = ServiceProvider!.GetService<ISoftDeleteDataFilter>()!;
        }
    }
}
