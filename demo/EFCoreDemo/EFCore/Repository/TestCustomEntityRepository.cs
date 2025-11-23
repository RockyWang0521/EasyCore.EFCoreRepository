using EasyCore.EFCoreRepository.Repository;
using EasyCore.EntityChange;
using EFCore.Entity;
using EFCoreDbContext.EntityFrameworkCore.EFDbContext;

namespace EFCore.Repository
{
    public class TestCustomEntityRepository :
        EfCoreRepository<TestDbContext, TestCustomEntity>,
        ITestCustomEntityRepository,
        IEntityAddedChangeHandler<TestCustomEntity>
    {
        public TestCustomEntityRepository(TestDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
        {

        }

        public async Task OnAddedAsync(TestCustomEntity entity)
        {
            if (entity is CustomEntity customEntity)
            {
                customEntity.CreateId = "Test";
            }

            await Task.CompletedTask;
        }
    }
}
