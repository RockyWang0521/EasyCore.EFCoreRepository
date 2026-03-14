using EasyCore.MongoDbRepository.DataFilter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyCore.MongoDbRepository
{
    public static class MongoDbRepository
    {
        public static void EasyCoreMongoDbRepository(this IServiceCollection services)
        {
            var registeredDbContexts = services.Where(d => typeof(DbContext).IsAssignableFrom(d.ServiceType)).Select(d => d.ServiceType).ToHashSet();

            if (!registeredDbContexts.Any()) return;

            services.AddHttpContextAccessor();

            services.TryAddTransient<ITenantFilter, TenantFilter>();

            services.TryAddTransient<ISoftDeleteFilter, SoftDeleteFilter>();
        }
    }
}
