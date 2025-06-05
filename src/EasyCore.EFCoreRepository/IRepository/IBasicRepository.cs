namespace EasyCore.EFCoreRepository.IRepository
{
    /// <summary>
    /// 基础仓储接口
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IBasicRepository<TEntity> : IReadOnlyBasicRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="autoSave">是否自动保存</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TEntity> InsertAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken));

        TEntity Insert(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 批量新增
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="autoSave"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task InsertManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken));

        void InsertMany(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="autoSave"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TEntity> UpdateAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken));

        TEntity Update(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="autoSave"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task UpdateManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken));

        void UpdateMany(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="autoSave"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteAsync(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken));

        void Delete(TEntity entity, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="autoSave"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DeleteManyAsync(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken));

        void DeleteMany(IEnumerable<TEntity> entities, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 保存更改
        /// </summary>
        /// <returns></returns>
        Task<int> SaveChangesAsync();

        int SaveChanges();
    }
}
