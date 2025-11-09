using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EasyCore.EFCoreRepository.IRepository
{
    /// <summary>
    /// Defines a read-only repository with basic query operations such as 
    /// retrieving lists, counts, and paged results.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    public interface IReadOnlyBasicRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<List<TEntity>> GetListAsync(         
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all entities.
        /// </summary>       
        List<TEntity> GetList();

        /// <summary>
        /// Gets the total number of entities.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<long> GetCountAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the total number of entities.
        /// </summary>
        long GetCount();

        /// <summary>
        /// Gets the number of entities that match the given predicate.
        /// </summary>
        /// <param name="predicate">Filter expression.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<long> GetCountAsync(
            [NotNull] Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the number of entities that match the given predicate.
        /// </summary>
        /// <param name="predicate">Filter expression.</param>
        long GetCount(
            [NotNull] Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Gets a paged list of entities.
        /// </summary>
        /// <param name="skipCount">Number of records to skip.</param>
        /// <param name="maxResultCount">Number of items to return.</param>               
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<List<TEntity>> GetPagedListAsync(
            int skipCount,
            int maxResultCount,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paged list of entities.
        /// </summary>
        /// <param name="skipCount">Number of records to skip.</param>
        /// <param name="maxResultCount">Number of items to return.</param>     
        List<TEntity> GetPagedList(
            int skipCount,
            int maxResultCount);
    }
}
