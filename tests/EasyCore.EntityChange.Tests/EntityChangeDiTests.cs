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

public class RecordingDeletedHandler : IEntityDeletedChangeHandler<ChangeEntity>
{
    public static int CallCount;

    public Task OnDeletedAsync(ChangeEntity entity)
    {
        CallCount++;
        return Task.CompletedTask;
    }
}

public class EntityChangeDiTests
{
    [Fact]
    public async Task SoftDelete_DispatchesDeletedHandler_WithoutBuildServiceProvider()
    {
        RecordingDeletedHandler.CallCount = 0;

        var services = new ServiceCollection();
        services.AddLogging();
        services.EasyCoreEntityChange()
            .AddHandler<RecordingDeletedHandler>();

        services.AddDbContext<ChangeDbContext>((sp, o) =>
        {
            o.UseInMemoryDatabase(Guid.NewGuid().ToString());
            o.UseEasyCoreEntityChange(sp);
        });

        await using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChangeDbContext>();

        var entity = new ChangeEntity { Id = Guid.NewGuid(), Name = "x" };
        db.Entities.Add(entity);
        await db.SaveChangesAsync();

        entity.IsDeleted = true;
        await db.SaveChangesAsync();

        Assert.Equal(1, RecordingDeletedHandler.CallCount);
    }
}
