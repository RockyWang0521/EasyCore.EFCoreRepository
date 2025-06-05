using EasyCore.EFCoreRepository.Repository;
using EFCore.Entity;
using EFCoreDbContext.EntityFrameworkCore.EFDbContext;

namespace EFCore.Repository
{
    public class TestEntityRepository : EfCoreRepository<TestDbContext, TestEntity>, ITestEntityRepository
    {
        public TestEntityRepository(TestDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
        {

        }
    }
}
