using EasyCore.EFCoreRepository.IRepository;
using EFCore.Entity;
using EasyCore.Dependencie;

namespace EFCore.Repository
{
    public interface ITestEntityRepository : IRepository<TestEntity>, ITransientDependencie
    {

    }
}
