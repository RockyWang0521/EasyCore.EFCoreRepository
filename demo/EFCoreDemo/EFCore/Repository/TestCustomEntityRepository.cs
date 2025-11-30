using EFCore.Entity;
using EFCore.Repository.CustomRepository;
using EFCoreDbContext.EntityFrameworkCore.EFDbContext;

namespace EFCore.Repository
{
    public class TestCustomEntityRepository : CustomEntityRepository<TestDbContext, TestCustomEntity>, ITestCustomEntityRepository
    {
        public TestCustomEntityRepository(TestDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
        {

        }
    }
}
