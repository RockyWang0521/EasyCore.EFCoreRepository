using EasyCore.EFCoreRepository.EntityBase;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EasyCore.EFCoreRepository.IRepository
{
    /// <summary>
    /// Defines a full-featured repository that supports read, write, filter,
    /// and predicate-based operations for a specific DbContext.
    /// </summary>
    /// <typeparam name="TDbContext">EF Core DbContext type.</typeparam>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    public interface IRepository<TDbContext, TEntity> :
        IRepository<TEntity>,
        IFilter<TDbContext, TEntity>
        where TDbContext : DbContext
        where TEntity : class, IEntity
    {    
        /// <summary>
        /// Gets the DbContext for this repository.
        /// </summary>
        /// <returns></returns>
        TDbContext GetDbContext();
    }

    /// <summary>
    /// Defines a full-featured repository that supports read, write, filter,
    /// and predicate-based operations for a specific entity type.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepository<TEntity> :
        IReadOnlyRepository<TEntity>,
        IBasicRepository<TEntity>
        where TEntity : class, IEntity
    {
        /// <summary>
        /// Gets a single entity matching the specified predicate.
        /// </summary>
        /// <param name="predicate">Filter expression.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The entity if found; otherwise null or exception depending on implementation.</returns>
        Task<TEntity> GetFirstAsync(
            [NotNull] Expression<Func<TEntity, bool>> predicate,

            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single entity matching the specified predicate.
        /// </summary>
        /// <param name="predicate">Filter expression.</param>    
        /// <returns>The entity if found; otherwise null.</returns>
        TEntity? GetFirst(
            [NotNull] Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets a queryable collection of entities.
        /// </summary>
        /// <returns></returns>
        IQueryable<TEntity> AsQueryable(
            [NotNull] Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets a queryable collection of entities.
        /// </summary>
        /// <returns></returns>
        IQueryable<TEntity> AsQueryable();

        /// <summary>
        /// Gets the entity database set.
        /// </summary>
        /// <returns></returns>
        DbSet<TEntity> EntityDbSet();

        /// <summary>
        /// Get a list of entities with paging.
        /// </summary>
        /// <param name="skipCount"></param>
        /// <param name="maxResultCount"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<TEntity>> GetPagedListAsync(
            int skipCount, int maxResultCount,
            [NotNull] Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a list of entities with paging.
        /// </summary>
        /// <param name="skipCount"></param>
        /// <param name="maxResultCount"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        List<TEntity> GetPagedList(
            int skipCount, int maxResultCount,
            [NotNull] Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Deletes entities matching the specified predicate.
        /// </summary>
        /// <param name="predicate">Filter expression.</param>
        /// <param name="autoSave">Whether to automatically save changes.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task DeleteAsync(
            [NotNull] Expression<Func<TEntity, bool>> predicate,
            bool autoSave = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes entities matching the specified predicate.
        /// </summary>
        /// <param name="predicate">Filter expression.</param>
        /// <param name="autoSave">Whether to automatically save changes.</param>
        void Delete(
            [NotNull] Expression<Func<TEntity, bool>> predicate,
            bool autoSave = false);

        /// <summary>
        /// Deletes entities matching the predicate without applying soft-delete
        /// or filters; directly removes data from the database.
        /// </summary>
        /// <param name="predicate">Filter expression.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task DeleteDirectAsync(
            [NotNull] Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes entities matching the predicate without applying soft-delete
        /// or filters; directly removes data from the database.
        /// </summary>
        /// <param name="predicate">Filter expression.</param>
        void DeleteDirect(
            [NotNull] Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Deletes the specified entities directly from the database without applying filters.
        /// </summary>
        /// <param name="entities">Entities to delete.</param>
        /// <param name="autoSave">Whether to automatically save changes.</param>
        void DeleteManyDirect(
            IEnumerable<TEntity> entities,
            bool autoSave = false);

        /// <summary>
        /// Deletes the specified entities directly from the database without applying filters.
        /// </summary>
        /// <param name="entities">Entities to delete.</param>
        /// <param name="autoSave">Whether to automatically save changes.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task DeleteManyDirectAsync(
            IEnumerable<TEntity> entities,
            bool autoSave = false,
            CancellationToken cancellationToken = default);
    }
}
