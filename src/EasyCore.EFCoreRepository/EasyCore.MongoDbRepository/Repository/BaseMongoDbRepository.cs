using EasyCore.MongoDbRepository.DataFilter;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.MongoDbRepository.Repository
{
    /// <summary>
    /// Base repository for all repositories.
    /// </summary>
    public class BaseMongoDbRepository
    {
        internal readonly IServiceProvider ServiceProvider;

        internal readonly ITenantFilter TenantFilter;

        internal readonly ISoftDeleteFilter SoftDeleteFilter;

        internal BaseMongoDbRepository(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            TenantFilter = ServiceProvider!.GetService<ITenantFilter>()!;
            SoftDeleteFilter = ServiceProvider!.GetService<ISoftDeleteFilter>()!;
        }

        internal IDataFilter GetDataFilter(Type filter)
        {
            if (filter == typeof(ISoftDeleteFilter))
                return SoftDeleteFilter;
            else if (filter == typeof(ITenantFilter))
                return TenantFilter;
            else
            {
                if (!typeof(IDataFilter).IsAssignableFrom(filter))
                    throw new ArgumentException("Invalid filter type");

                return (IDataFilter)ServiceProvider.GetService(filter)!;
            }
        }
    }
}
