using Castle.DynamicProxy;
using EasyCore.UnitOfWork;
using EasyCore.UnitOfWork.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyCore.UnitOfWork.Tests;

public class UowEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UowDbContext : DbContext
{
    public int SaveChangesAsyncCallCount { get; private set; }

    public UowDbContext(DbContextOptions<UowDbContext> options) : base(options)
    {
    }

    public DbSet<UowEntity> Entities => Set<UowEntity>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesAsyncCallCount++;
        return base.SaveChangesAsync(cancellationToken);
    }
}

public interface IUowService
{
    Task InsertAsync();
}

public class UowService : IUowService
{
    private readonly UowDbContext _db;

    public UowService(UowDbContext db) => _db = db;

    [SaveChanges(typeof(UowDbContext))]
    public Task InsertAsync()
    {
        _db.Entities.Add(new UowEntity { Id = Guid.NewGuid(), Name = "uow" });
        return Task.CompletedTask;
    }
}

public class SaveChangesInterceptorAsyncTests
{
    [Fact]
    public async Task Interceptor_CallsSaveChangesAsync()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<UowDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.EasyCoreUnitOfWork()
            .RegisterSaveChangesFor<IUowService, UowService>();

        await using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IUowService>();
        var db = scope.ServiceProvider.GetRequiredService<UowDbContext>();

        await svc.InsertAsync();

        Assert.True(db.SaveChangesAsyncCallCount >= 1);
        Assert.Single(await db.Entities.ToListAsync());
    }
}
