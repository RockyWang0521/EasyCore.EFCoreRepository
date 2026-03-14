using EasyCore.EFCoreRepository.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace EasyCore.EFCoreRepository.Tests.Filters;

public class SoftDeleteFilterTests
{
    [Fact]
    public async Task SoftDeleteFilter_ExcludesDeletedEntities()
    {
        await using var sp = TestServiceFactory.Create();
        var repo = TestServiceFactory.CreateRepo<SoftDeleteEntity>(sp);

        var a = await repo.InsertAsync(new SoftDeleteEntity { Id = Guid.NewGuid(), Name = "a" }, autoSave: true);
        await repo.InsertAsync(new SoftDeleteEntity { Id = Guid.NewGuid(), Name = "b" }, autoSave: true);
        await repo.DeleteAsync(a, autoSave: true);

        var list = await repo.GetListAsync();
        Assert.Single(list);
        Assert.Equal("b", list[0].Name);
    }
}

public class TenantFilterTests
{
    [Fact]
    public async Task TenantFilter_StampsTenantOnInsert()
    {
        await using var sp = TestServiceFactory.Create("tenant-a");
        var repo = TestServiceFactory.CreateRepo<TenantEntity>(sp);

        var entity = await repo.InsertAsync(new TenantEntity { Id = Guid.NewGuid(), Name = "a1" }, autoSave: true);

        Assert.Equal("tenant-a", entity.TenantId);
        var list = await repo.GetListAsync();
        Assert.Single(list);
        Assert.Equal("tenant-a", list[0].TenantId);
    }

    [Fact]
    public async Task TenantFilter_EmptyTenant_ReturnsNoRows()
    {
        await using var sp = TestServiceFactory.Create(tenantId: null);
        var repo = TestServiceFactory.CreateRepo<TenantEntity>(sp);
        var db = sp.GetRequiredService<TestDbContext>();

        db.TenantEntities.Add(new TenantEntity { Id = Guid.NewGuid(), Name = "leak", TenantId = "other" });
        await db.SaveChangesAsync();

        Assert.Empty(await repo.GetListAsync());
    }
}
