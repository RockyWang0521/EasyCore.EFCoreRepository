using EasyCore.MongoDbRepository.Repository;
using MongoDb.Entity;
using MongoDbContext.EntityFrameworkCore.EFDbContext;

namespace MongoDb.Repository
{
    public class TestEntityRepository : MongoDbRepository<TestDbContext, TestEntity>, ITestEntityRepository
    {
        public TestEntityRepository(TestDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
        {

        }
    }
}
