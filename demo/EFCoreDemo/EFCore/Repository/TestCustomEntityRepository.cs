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

        public List<IDataFilter> TestCustomEntityGetDataFilters()
        {
            var aaa = DataFilters;

            //AddOnce(DataFilters, typeof(CustomDataFilter));

            aaa = DataFilters;

            //this.AsQueryable().Where(x => x.Id == Guid.NewGuid());

            return DataFilters;
        }
    }
}
