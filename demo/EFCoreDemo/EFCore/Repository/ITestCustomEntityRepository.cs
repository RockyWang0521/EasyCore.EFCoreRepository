using EasyCore.Dependencie;
using EasyCore.EFCoreRepository.DataFilter;
using EasyCore.EFCoreRepository.IRepository;
using EFCore.Entity;
using EFCoreDbContext.EntityFrameworkCore.EFDbContext;

namespace EFCore.Repository
{
    public interface ITestCustomEntityRepository : IRepository<TestDbContext,TestCustomEntity>, ITransientDependencie
    {
        /// <summary>
        ///  Get the data filters for the TestCustomEntity.
        /// </summary>
        /// <returns></returns>
        List<IDataFilter> TestCustomEntityGetDataFilters();
    }
}
