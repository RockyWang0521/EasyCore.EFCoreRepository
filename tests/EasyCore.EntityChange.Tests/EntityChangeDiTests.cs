using EasyCore.EntityChange;
using EasyCore.EntityChange.EntityChange;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.EntityChange.Tests;

public class ChangeEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
}

public class ChangeDbContext : DbContext
{
    public ChangeDbContext(DbContextOptions<ChangeDbContext> options) : base(options)
    {
    }

    public DbSet<ChangeEntity> Entities => Set<ChangeEntity>();
}

public class RecordingHandlers :
    IEntityAddedChangeHandler<ChangeEntity>,
    IEntityUpdatedChangeHandler<ChangeEntity>,
    IEntityDeletedChangeHandler<ChangeEntity>
{
    public static int AddedCount;
    public static int UpdatedCount;
    public static int DeletedCount;

    public static void Reset()
    {
        AddedCount = 0;
        UpdatedCount = 0;
        DeletedCount = 0;
    }

    public Task OnAddedAsync(ChangeEntity entity)
    {
        AddedCount++;
        return Task.CompletedTask;
    }

    public Task OnUpdatedAsync(ChangeEntity oldEntity, ChangeEntity newEntity)
    {
        UpdatedCount++;
        return Task.CompletedTask;
    }

    public Task OnDeletedAsync(ChangeEntity entity)
    {
        DeletedCount++;
        return Task.CompletedTask;
    }
}

public class EntityChangeDiTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddDbContext<ChangeDbContext>(o =>
            o.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddEasyCoreEntityChange()
            .AddHandler<RecordingHandlers>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task SoftDelete_DispatchesDeletedHandler_WithoutBuildServiceProvider()
    {
        RecordingHandlers.Reset();

        await using var sp = BuildProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChangeDbContext>();

        var entity = new ChangeEntity { Id = Guid.NewGuid(), Name = "x" };
        db.Entities.Add(entity);
        await db.SaveChangesAsync();

        entity.IsDeleted = true;
        await db.SaveChangesAsync();

        Assert.Equal(1, RecordingHandlers.AddedCount);
        Assert.Equal(1, RecordingHandlers.DeletedCount);
        Assert.Equal(0, RecordingHandlers.UpdatedCount);
    }

    [Fact]
    public void Sync_InsertUpdateSoftDelete_DispatchesAllHandlers()
    {
        RecordingHandlers.Reset();

        using var sp = BuildProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChangeDbContext>();

        var entity = new ChangeEntity { Id = Guid.NewGuid(), Name = "x" };
        db.Entities.Add(entity);
        db.SaveChanges();
        Assert.Equal(1, RecordingHandlers.AddedCount);

        entity.Name = "y";
        db.SaveChanges();
        Assert.Equal(1, RecordingHandlers.UpdatedCount);

        entity.IsDeleted = true;
        db.SaveChanges();
        Assert.Equal(1, RecordingHandlers.DeletedCount);
        Assert.Equal(1, RecordingHandlers.UpdatedCount);
    }
}
