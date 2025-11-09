using EasyCore.MongoDbRepository.EntityBase;
using EasyCore.MongoDbRepository.Repository;
using Microsoft.EntityFrameworkCore;

namespace EasyCore.MongoDbRepository.IRepository
{
    /// <summary>
    /// Defines methods to dynamically add or remove data filters
    /// (e.g., soft-delete, tenant filters) on a repository for a specific DbContext.
    /// </summary>
    /// <typeparam name="TDbContext">EF Core DbContext type.</typeparam>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    public interface IFilter<TDbContext, TEntity>
        where TDbContext : DbContext
        where TEntity : class, IEntity
    {
        /// <summary>
        /// Adds a filter of the specified type to the repository.
        /// </summary>
        /// <param name="filterType">The type of the filter to add.</param>
        /// <returns>The repository instance with the filter applied.</returns>
        MongoDbRepository<TDbContext, TEntity> AddFilter(
            Type filterType);

        /// <summary>
        /// Removes a filter of the specified type from the repository.
        /// </summary>
        /// <param name="filterType">The type of the filter to remove.</param>
        /// <returns>The repository instance with the filter removed.</returns>
        MongoDbRepository<TDbContext, TEntity> RemoveFilter(
            Type filterType);
    }
}