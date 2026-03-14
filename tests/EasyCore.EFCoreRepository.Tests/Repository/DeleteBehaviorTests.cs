using EasyCore.EFCoreRepository.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.EFCoreRepository.Tests.Repository;

public class DeleteBehaviorTests
{
    [Fact]
    public async Task Delete_HardOnlyEntity_RemovesRow()
    {
        await using var sp = TestServiceFactory.Create();
        var repo = TestServiceFactory.CreateRepo<HardOnlyEntity>(sp);
        var db = sp.GetRequiredService<TestDbContext>();

        var entity = new HardOnlyEntity { Id = Guid.NewGuid(), Name = "hard" };
        db.HardOnlyEntities.Add(entity);
        await db.SaveChangesAsync();

        await repo.DeleteAsync(entity, autoSave: true);

        Assert.Empty(await db.HardOnlyEntities.ToListAsync());
    }

    [Fact]
    public async Task Delete_SoftDeleteEntity_MarksDeletedAndFiltersOut()
    {
        await using var sp = TestServiceFactory.Create();
        var repo = TestServiceFactory.CreateRepo<SoftDeleteEntity>(sp);

        var entity = await repo.InsertAsync(new SoftDeleteEntity
        {
            Id = Guid.NewGuid(),
            Name = "soft"
        }, autoSave: true);

        await repo.DeleteAsync(entity, autoSave: true);

        Assert.Empty(await repo.GetListAsync());
        Assert.True(entity.IsDeleted);
    }

    [Fact]
    public async Task DeleteDirect_FindsSoftDeletedEntity()
    {
        await using var sp = TestServiceFactory.Create();
        var repo = TestServiceFactory.CreateRepo<SoftDeleteEntity>(sp);

        var id = Guid.NewGuid();
        await repo.InsertAsync(new SoftDeleteEntity { Id = id, Name = "x" }, autoSave: true);
        var entity = await repo.GetFirstAsync(e => e.Id == id);
        await repo.DeleteAsync(entity, autoSave: true);

        Assert.Empty(await repo.GetListAsync());

        await repo.DeleteDirectAsync(e => e.Id == id);

        var db = sp.GetRequiredService<TestDbContext>();
        Assert.Empty(await db.SoftDeleteEntities.IgnoreQueryFilters().ToListAsync());
    }
}
