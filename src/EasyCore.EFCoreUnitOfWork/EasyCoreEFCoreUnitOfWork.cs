using Castle.DynamicProxy;
using EasyCore.EFCoreUnitOfWork.EFCoreUnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace EasyCore.EFCoreUnitOfWork
{
    /// <summary>
    /// Automatically save changes to the database for methods marked with the SaveChangesAttribute.
    /// </summary>
    public static class EasyCoreUnitOfWork
    {
        public static void EasyCoreEFCoreUnitOfWork(this IServiceCollection service)
        {
            service.AddScoped<IAsyncInterceptor, SaveChangesStandardInterceptorAsync>();

            service.TryAddSingleton<ProxyGenerator>();

            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;

            string[] dllFiles = Directory.GetFiles(rootDirectory, "*.dll", SearchOption.TopDirectoryOnly).Where(path =>
            {
                string fileName = Path.GetFileName(path);
                return !(fileName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) || fileName.StartsWith("System.", StringComparison.OrdinalIgnoreCase));
            }).ToArray();

            List<Type> types = new List<Type>();

            foreach (var dll in dllFiles)
            {
                Assembly assembly = Assembly.LoadFrom(dll);

                foreach (var type in assembly.GetTypes())
                {
                    bool hasClassAttribute = type.GetCustomAttributes(typeof(SaveChangesAttribute), inherit: false).Any();

                    bool hasMethodAttribute = type.GetMethods().Any(m => m.GetCustomAttributes(typeof(SaveChangesAttribute), inherit: false).Any());

                    if (hasClassAttribute || hasMethodAttribute)
                    {
                        service.AddTransient(type);

                        var interfaceType = type.GetInterface("I" + type.Name);

                        if (interfaceType is null) continue;

                        if (interfaceType != null)
                        {
                            service.AddTransient(interfaceType, serviceProvider =>
                            {
                                var generator = serviceProvider.GetRequiredService<ProxyGenerator>();

                                var instance = serviceProvider.GetRequiredService(type);

                                var interceptors = serviceProvider.GetServices<IAsyncInterceptor>().ToArray();

                                return generator.CreateInterfaceProxyWithTarget(interfaceType, instance, interceptors);
                            });
                        }
                    }
                }
            }
        }
    }
}
