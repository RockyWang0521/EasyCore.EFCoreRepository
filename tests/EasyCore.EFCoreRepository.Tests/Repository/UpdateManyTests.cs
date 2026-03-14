using EasyCore.EFCoreRepository.Tests.Fixtures;

namespace EasyCore.EFCoreRepository.Tests.Repository;

public class UpdateManyTests
{
    [Fact]
    public async Task UpdateManyAsync_SetsUpdateTimeAndConcurrencyStamp()
    {
        await using var sp = TestServiceFactory.Create();
        var repo = TestServiceFactory.CreateRepo<SoftDeleteEntity>(sp);

        var entity = await repo.InsertAsync(new SoftDeleteEntity
        {
            Id = Guid.NewGuid(),
            Name = "before"
        }, autoSave: true);

        var originalStamp = entity.ConcurrencyStamp;
        entity.Name = "after";

        await repo.UpdateManyAsync(new[] { entity }, autoSave: true);

        var loaded = await repo.GetFirstAsync(e => e.Id == entity.Id);
        Assert.Equal("after", loaded.Name);
        Assert.NotNull(loaded.UpdateTime);
        Assert.NotEqual(originalStamp, loaded.ConcurrencyStamp);
    }
}
