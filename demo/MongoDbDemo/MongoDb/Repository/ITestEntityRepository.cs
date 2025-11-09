using EasyCore.Dependencie;
using EasyCore.MongoDbRepository.IRepository;
using MongoDb.Entity;
using MongoDbContext.EntityFrameworkCore.EFDbContext;

namespace MongoDb.Repository
{
    public interface ITestEntityRepository : IRepository<TestDbContext, TestEntity>, ITransientDependencie
    {

    }
}
