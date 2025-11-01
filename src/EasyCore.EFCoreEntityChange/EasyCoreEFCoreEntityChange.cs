using EasyCore.EFCoreEntityChange.EntityChange;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EasyCore.EFCoreEntityChange
{
    public static class EFCoreEntityChange
    {
        public static void EasyCoreEFCoreEntityChange<TContext>(this IServiceCollection services) where TContext : DbContext
        {
            services.AddScoped<IInterceptor, EntityChangeInterceptor>();

            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string[] dllFiles = Directory.GetFiles(rootDirectory, "*.dll", SearchOption.TopDirectoryOnly).Where(path =>
            {
                string fileName = Path.GetFileName(path);
                return !(fileName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) || fileName.StartsWith("System.", StringComparison.OrdinalIgnoreCase));
            }).ToArray();

            foreach (string dllFile in dllFiles)
            {
                Assembly assembly = Assembly.LoadFrom(dllFile);

                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (!type.IsClass || type.IsAbstract) continue;

                    var implements = type.GetInterfaces().Contains(typeof(IEntityChangeHandler));

                    if (!implements) continue;

                    var interfaces = type.GetInterfaces();

                    foreach (var @interface in interfaces)
                    {
                        if (!@interface.IsGenericType) continue;

                        if (@interface.GetGenericTypeDefinition() == typeof(IEntityAddedChangeHandler<>))
                        {
                            services.AddScoped(typeof(IEntityAddedChangeHandler<>).MakeGenericType(@interface.GetGenericArguments()[0]), type);

                            continue;
                        }
                        else if (@interface.GetGenericTypeDefinition() == typeof(IEntityUpdatedChangeHandler<,>))
                        {
                            services.AddScoped(typeof(IEntityUpdatedChangeHandler<,>).MakeGenericType(@interface.GetGenericArguments()[0], @interface.GetGenericArguments()[1]), type);

                            continue;
                        }
                        else if (@interface.GetGenericTypeDefinition() == typeof(IEntityDeletedChangeHandler<>))
                        {
                            services.AddScoped(typeof(IEntityDeletedChangeHandler<>).MakeGenericType(@interface.GetGenericArguments()[0]), type);

                            continue;
                        }
                    }
                }
            }
        }
    }
}
