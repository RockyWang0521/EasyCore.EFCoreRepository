using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EasyCore.MongoDbRepository.IRepository
{
    /// <summary>
    /// Extends the read-only repository with query methods that support
    /// filtering using expression predicates.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    public interface IReadOnlyRepository<TEntity> : IReadOnlyBasicRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Gets a list of entities matching the specified predicate.
        /// </summary>
        /// <param name="predicate">Filter expression.</param>      
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Filtered list of entities.</returns>
        Task<List<TEntity>> GetListAsync(
            [NotNull] Expression<Func<TEntity, bool>> predicate,          
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a list of entities matching the specified predicate.
        /// </summary>
        /// <param name="predicate">Filter expression.</param>       
        /// <returns>Filtered list of entities.</returns>
        List<TEntity> GetList(
            [NotNull] Expression<Func<TEntity, bool>> predicate);
    }
}
