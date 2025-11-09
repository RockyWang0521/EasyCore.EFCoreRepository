using EasyCore.EFCoreRepository.IRepository;
using EFCore.Entity;
using EasyCore.Dependencie;
using EFCoreDbContext.EntityFrameworkCore.EFDbContext;

namespace EFCore.Repository
{
    public interface ITestEntityRepository : IRepository<TestDbContext, TestEntity>, ITransientDependencie
    {

    }
}
