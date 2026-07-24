using EasyCore.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

/// <summary>
/// Class-only proxy sample: HandleAsync must be virtual for Castle to intercept.
/// </summary>
public class UowEventHandler
{
    private readonly UowDbContext _db;

    public UowEventHandler(UowDbContext db) => _db = db;

    [SaveChanges(typeof(UowDbContext))]
    public virtual Task HandleAsync(string payload)
    {
        _db.Entities.Add(new UowEntity { Id = Guid.NewGuid(), Name = payload });
        return Task.CompletedTask;
    }
}

public class SaveChangesProxyTests
{
    private static ServiceProvider BuildProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<UowDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        // Register first, then UnitOfWork ApplyProxies.
        services.AddTransient<IUowService, UowService>();
        services.AddTransient<UowService>();
        services.AddTransient<UowEventHandler>();
        services.AddEasyCoreUnitOfWork();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Proxy_CallsSaveChangesAsync_OnDirectCall()
    {
        await using var sp = BuildProvider();
        using var scope = sp.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IUowService>();
        var db = scope.ServiceProvider.GetRequiredService<UowDbContext>();

        await svc.InsertAsync();

        Assert.True(db.SaveChangesAsyncCallCount >= 1);
        Assert.Single(await db.Entities.ToListAsync());
    }

    [Fact]
    public async Task Proxy_CallsSaveChangesAsync_OnReflectionInvoke_HandleAsync()
    {
        await using var sp = BuildProvider();
        using var scope = sp.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<UowEventHandler>();
        var db = scope.ServiceProvider.GetRequiredService<UowDbContext>();

        var method = handler.GetType().GetMethod(nameof(UowEventHandler.HandleAsync))!;
        var task = (Task)method.Invoke(handler, new object[] { "evt" })!;
        await task;

        Assert.True(db.SaveChangesAsyncCallCount >= 1);
        Assert.Single(await db.Entities.ToListAsync());
        Assert.Equal("evt", (await db.Entities.SingleAsync()).Name);
    }
}
