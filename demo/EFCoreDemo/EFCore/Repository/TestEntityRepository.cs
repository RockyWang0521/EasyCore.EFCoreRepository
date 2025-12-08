using EasyCore.EFCoreRepository.DataFilter;
using EasyCore.EFCoreRepository.Demo.DataFilter;
using EasyCore.EFCoreRepository.Repository;
using EFCore.Entity;
using EFCoreDbContext.EntityFrameworkCore.EFDbContext;

namespace EFCore.Repository
{
    public class TestEntityRepository :
        EfCoreRepository<TestDbContext, TestEntity>,
        ITestEntityRepository
    {
        public TestEntityRepository(TestDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
        {

        }

        /// <summary>
        /// Applies permanent data filters before persisting entities (Insert/Update/Delete).
        /// This method is called during the persistence pipeline to enforce global filters.
        /// </summary>
        /// <param name="dataFilters">The list of data filters that are currently scheduled for execution.</param>
        /// <returns>The updated list of data filters after applying permanent filter rules.</returns>
        public override List<IDataFilter> OnApplyPersistingFilters(List<IDataFilter> dataFilters)
        {
            //AddOnce(dataFilters, typeof(CustomDataFilter));

            //RemoveIfExistsFilter(dataFilters, typeof(CustomDataFilter));

            return dataFilters;
        }
    }
}
