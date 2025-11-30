using EasyCore.MongoDbRepository.Repository;
using EasyCore.MongoDbRepository.DataFilter;
using EasyCore.MongoDbRepository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

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

            var rootDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string[] dllFiles = Directory.GetFiles(rootDirectory, "*.dll", SearchOption.TopDirectoryOnly).Where(path =>
            {
                string fileName = Path.GetFileName(path);
                return !(fileName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) || fileName.StartsWith("System.", StringComparison.OrdinalIgnoreCase));
            }).ToArray();

            var repoImplBaseType = typeof(MongoDbRepository<,>);

            var repoInterfaceType = typeof(IMongoDbRepository<,>);

            foreach (var dll in dllFiles)
            {
                var assembly = Assembly.LoadFrom(dll);

                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsClass || type.IsAbstract) continue;

                    var baseType = type.BaseType;

                    if (baseType == null || !baseType.IsGenericType) continue;

                    var baseGeneric = baseType.GetGenericTypeDefinition();

                    if (baseGeneric != repoImplBaseType) continue;

                    var args = baseType.GetGenericArguments();

                    var dbContextType = args[0];

                    var entityType = args[1];

                    if (!registeredDbContexts.Contains(dbContextType)) continue;

                    var interfaceType = repoInterfaceType.MakeGenericType(dbContextType, entityType);

                    services.AddTransient(interfaceType, type);
                }
            }
        }
    }
}
