using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EasyCore.EFCoreRepository.IRepository
{
    /// <summary>
    /// 只读基础仓储接口
    /// includeDetails--返回实体时是否包含实体中的子实体
    /// 异步&同步方法
    /// </summary>
    /// <typeparam name="TEntity">实体</typeparam>
    public interface IReadOnlyBasicRepository<TEntity>  where TEntity : class
    {
        /// <summary>
        /// 获取所有实体对象
        /// </summary>
        /// <param name="includeDetails"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<TEntity>> GetListAsync(bool includeDetails = false, CancellationToken cancellationToken = default(CancellationToken));
        List<TEntity> GetList(bool includeDetails = false, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 获取Count
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<long> GetCountAsync(CancellationToken cancellationToken = default(CancellationToken));
        long GetCount(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 获取指定的Count
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<long> GetCountAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        long GetCount([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="skipCount">跳过的数量</param>
        /// <param name="maxResultCount">最大返回数量</param>
        /// <param name="sorting">排序字段</param>
        /// <param name="includeDetails"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<List<TEntity>> GetPagedListAsync(int skipCount, int maxResultCount, string? sorting = null, bool includeDetails = false, CancellationToken cancellationToken = default(CancellationToken));
        List<TEntity> GetPagedList(int skipCount, int maxResultCount, string? sorting = null, bool includeDetails = false, CancellationToken cancellationToken = default(CancellationToken));
    }
}
