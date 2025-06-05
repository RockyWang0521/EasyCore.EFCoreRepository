using EasyCore.EFCoreRepository.DataFilter.SoftDeleteFilter;
using EasyCore.EFCoreRepository.DataFilter.TenantFilter;
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
