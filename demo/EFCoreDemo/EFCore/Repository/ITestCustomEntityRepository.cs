using EasyCore.Dependencie;
using EasyCore.EFCoreRepository.IRepository;
using EFCore.Entity;
using EFCoreDbContext.EntityFrameworkCore.EFDbContext;

namespace EFCore.Repository
{
    public interface ITestCustomEntityRepository : IRepository<TestDbContext, TestCustomEntity>, ITransientDependencie
    {

    }
}
