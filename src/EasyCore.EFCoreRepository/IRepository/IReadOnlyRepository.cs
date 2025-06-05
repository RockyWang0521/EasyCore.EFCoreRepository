using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EasyCore.EFCoreRepository.IRepository
{
    /// <summary>
    /// 只读仓储接口(向下兼容，继承子基础接口。引用时只需要引用此接口就行)
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IReadOnlyRepository<TEntity> : IReadOnlyBasicRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 可用Lambda表达式的查询
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="includeDetails"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<TEntity>> GetListAsync(
                [NotNull] Expression<Func<TEntity, bool>> predicate,
                bool includeDetails = false,
                CancellationToken cancellationToken = default);

        List<TEntity> GetList(
                [NotNull] Expression<Func<TEntity, bool>> predicate,
                bool includeDetails = false,
                CancellationToken cancellationToken = default);
    }
}
