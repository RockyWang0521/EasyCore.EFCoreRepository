using EasyCore.EFCoreRepository.IRepository;
using EFCore.Entity;
using EasyCore.Dependencie;
using EasyCore.EFCoreRepository.DataFilter;

namespace EFCore.Repository
{
    public interface ITestEntityRepository : IRepository<TestEntity>, ITransientDependencie
    {
        List<IDataFilter> TestCustomEntityGetDataFilters();
    }
}
