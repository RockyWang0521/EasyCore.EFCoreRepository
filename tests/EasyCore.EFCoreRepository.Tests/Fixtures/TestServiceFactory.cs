using EasyCore.EFCoreRepository.DataFilter;
using EasyCore.EFCoreRepository.Repository;
using EasyCore.EFCoreRepository.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.EFCoreRepository.Tests;

internal static class TestServiceFactory
{
    public static ServiceProvider Create(string? tenantId = "tenant-a", Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        var dbName = Guid.NewGuid().ToString();

        services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase(dbName));
        services.AddSingleton<ITenantProvider>(new FixedTenantProvider(tenantId));
        services.EasyCoreEFCoreRepository();
        configure?.Invoke(services);

        return services.BuildServiceProvider();
    }

    public static EfCoreRepository<TestDbContext, TEntity> CreateRepo<TEntity>(IServiceProvider sp)
        where TEntity : class, EntityBase.IEntity
    {
        return new EfCoreRepository<TestDbContext, TEntity>(
            sp.GetRequiredService<TestDbContext>(),
            sp);
    }
}

internal sealed class FixedTenantProvider : ITenantProvider
{
    private readonly string? _tenantId;

    public FixedTenantProvider(string? tenantId) => _tenantId = tenantId;

    public string? GetTenantId() => _tenantId;
}
