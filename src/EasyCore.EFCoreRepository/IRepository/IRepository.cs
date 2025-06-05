using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EasyCore.EFCoreRepository.IRepository
{
    /// <summary>
    /// 仓储接口
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepository<TEntity> : IReadOnlyRepository<TEntity>, IBasicRepository<TEntity>
    where TEntity : class
    {     
        Task<TEntity> GetAsync(
            [NotNull] Expression<Func<TEntity, bool>> predicate,
            bool includeDetails = false,
            CancellationToken cancellationToken = default);
        TEntity? Get(
            [NotNull] Expression<Func<TEntity, bool>> predicate,
            bool includeDetails = false,
            CancellationToken cancellationToken = default);
        Task DeleteAsync(
            [NotNull] Expression<Func<TEntity, bool>> predicate,
            bool autoSave = false,
            CancellationToken cancellationToken = default);
        void Delete(
            [NotNull] Expression<Func<TEntity, bool>> predicate,
            bool autoSave = false,
            CancellationToken cancellationToken = default);
        Task DeleteDirectAsync(
            [NotNull] Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default );
        void DeleteDirect(
            [NotNull] Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);
    }
}
