using EasyCore.EFCoreRepository.DataFilter.SoftDelete;
using EasyCore.EFCoreRepository.DataFilter.Tenant;
using EasyCore.EFCoreRepository.IRepository;
using EasyCore.EFCoreRepository.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace EasyCore.EFCoreRepository
{
    public static class EFCoreRepository
    {
        public static void EasyCoreEFCoreRepository(this IServiceCollection services)
        {
            var registeredDbContexts = services.Where(d => typeof(DbContext).IsAssignableFrom(d.ServiceType)).Select(d => d.ServiceType).ToHashSet();

            if (!registeredDbContexts.Any()) return;

            services.AddHttpContextAccessor();

            services.TryAddTransient<ITenantDataFilter, TenantDataFilter>();

            services.TryAddTransient<ISoftDeleteDataFilter, SoftDeleteDataFilter>();

            var rootDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var dlls = Directory.GetFiles(rootDirectory, "*.dll");

            var repoImplBaseType = typeof(EfCoreRepository<,>);

            var repoInterfaceType = typeof(IEfCoreRepository<>);

            foreach (var dll in dlls)
            {
                try
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

                        var interfaceType = repoInterfaceType.MakeGenericType(entityType);

                        services.AddTransient(interfaceType, type);
                    }
                }
                catch { }
            }
        }
    }
}
