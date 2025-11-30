using EasyCore.EntityChange.EntityChange;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EasyCore.EntityChange
{
    public static class DataBaseEntityChange
    {
        public static void EasyCoreEntityChange(this IServiceCollection services)
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

                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsClass || type.IsAbstract) continue;

                    var genericInterfaces = type.GetInterfaces()
                        .Where(i => i.IsGenericType)
                        .ToArray();

                    foreach (var @interface in genericInterfaces)
                    {
                        var genericDef = @interface.GetGenericTypeDefinition();

                        if (genericDef == typeof(IEntityAddedChangeHandler<>)
                            || genericDef == typeof(IEntityUpdatedChangeHandler<>)
                            || genericDef == typeof(IEntityDeletedChangeHandler<>))
                        {
                            // 泛型安全注册
                            services.AddScoped(@interface, type);
                        }
                    }
                }
            }
        }

        public static void UseEasyCoreEntityChange(this DbContextOptionsBuilder builder, IServiceCollection services) =>
            builder.AddInterceptors(new EntityChangeInterceptor(services.BuildServiceProvider()));
    }
}
