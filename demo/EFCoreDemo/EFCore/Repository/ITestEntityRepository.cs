using EasyCore.Dependencie;
using EasyCore.EFCoreRepository.DataFilter;
using EasyCore.EFCoreRepository.IRepository;
using EFCore.Entity;
using EFCoreDbContext.EntityFrameworkCore.EFDbContext;

namespace EFCore.Repository
{
    public interface ITestEntityRepository : IRepository<TestDbContext, TestEntity>, ITransientDependencie
    {
        List<IDataFilter> TestCustomEntityGetDataFilters();
    }
}
