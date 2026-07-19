using EasyCore.MongoDbRepository.DataFilter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyCore.MongoDbRepository
{
    public static class MongoDbRepository
    {
        public static void AddEasyCoreMongoDbRepository(this IServiceCollection services)
        {
            TryAttachEntityChange(services);

            var registeredDbContexts = services
                .Where(d => typeof(DbContext).IsAssignableFrom(d.ServiceType))
                .Select(d => d.ServiceType)
                .ToHashSet();

            if (!registeredDbContexts.Any()) return;

            services.AddHttpContextAccessor();

            services.TryAddSingleton<ITenantProvider, HttpContextTenantProvider>();
            services.TryAddTransient<ITenantFilter, TenantFilter>();
            services.TryAddTransient<ISoftDeleteFilter, SoftDeleteFilter>();
        }

        /// <summary>
        /// Soft-calls EasyCore.EntityChange when that package is referenced, so
        /// <c>UseEasyCoreEntityChange</c> is not required in AddDbContext.
        /// </summary>
        private static void TryAttachEntityChange(IServiceCollection services)
        {
            try
            {
                var type = Type.GetType("EasyCore.EntityChange.DataBaseEntityChange, EasyCore.EntityChange", throwOnError: false);
                var method = type?.GetMethod(
                    "AttachEntityChangeToRegisteredDbContexts",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                method?.Invoke(null, new object[] { services });
            }
            catch
            {
                // EntityChange not loaded — ignore.
            }
        }
    }
}
