using EasyCore.EFCoreRepository.DataFilter;
using EasyCore.EFCoreRepository.Demo.DataFilter;
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

        /// <summary>
        /// You can also add data filters in the repository layer.
        /// </summary>
        /// <returns></returns>
        public List<IDataFilter> TestCustomEntityGetDataFilters()
        {
            var aaa = DataFilters;

            AddOnce(DataFilters, typeof(CustomDataFilter));

            aaa = DataFilters;

            return DataFilters;
        }
    }
}
