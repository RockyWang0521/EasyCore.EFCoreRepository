using EasyCore.EFCoreRepository.DataFilter;
using EasyCore.EFCoreRepository.EntityBase;
using EasyCore.EFCoreRepository.Repository;
using EFCore.Entity;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Repository.CustomRepository
{
    public class CustomEntityRepository<TDbContext, TEntity> : EfCoreRepository<TDbContext, TEntity>
        where TDbContext : DbContext
        where TEntity : class, IEntity
    {
        public CustomEntityRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
        {

        }

        /// <summary>
        /// Custom method to set the CreateId property of the entity before adding it to the database.
        /// </summary>
        /// <param name="entity"></param>
        public override void OnBeforeAdd(TEntity entity)
        {
            if (entity is CustomEntity customEntity)
            {
                customEntity.CreateId = "Test";
            }
        }

        /// <summary>
        /// Custom method to set the UpdateId property of the entity before updating it in the database.
        /// </summary>
        /// <param name="entity"></param>
        public override void OnBeforeUpdate(TEntity entity)
        {
            // Do something entity before update
        }

        /// <summary>
        /// Custom method to set the DeleteId property of the entity before deleting it from the database.
        /// </summary>
        /// <param name="entity"></param>
        public override void OnBeforeDelete(TEntity entity)
        {
            // Do something entity before delete
        }

        /// <summary>
        /// Applies permanent data filters before persisting entities (Insert/Update/Delete).
        /// This method is called during the persistence pipeline to enforce global filters.
        /// </summary>
        /// <param name="dataFilters">The list of data filters that are currently scheduled for execution.</param>
        /// <returns>The updated list of data filters after applying permanent filter rules.</returns>
        public override List<IDataFilter> OnApplyPersistingFilters(List<IDataFilter> dataFilters)
        {
            // Do something before persist

            return dataFilters;
        }
    }
}
