namespace EasyCore.EFCoreRepository.IRepository
{
    /// <summary>
    /// Defines a basic repository that supports CRUD operations,
    /// extending the read-only repository with insert, update, delete,
    /// and save-changes functionality.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    public interface IBasicRepository<TEntity> : IReadOnlyBasicRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Inserts a new entity.
        /// </summary>
        /// <param name="entity">Entity to insert.</param>
        /// <param name="autoSave">Whether to automatically save changes.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<TEntity> InsertAsync(
            TEntity entity, bool autoSave = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts a new entity.
        /// </summary>
        /// <param name="entity">Entity to insert.</param>
        /// <param name="autoSave">Whether to automatically save changes.</param>
        TEntity Insert(
            TEntity entity, bool autoSave = false);

        /// <summary>
        /// Inserts multiple entities.
        /// </summary>
        /// <param name="entities">Entities to insert.</param>
        /// <param name="autoSave">Whether to automatically save changes.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task InsertManyAsync(
            IEnumerable<TEntity> entities, bool autoSave = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts multiple entities.
        /// </summary>
        /// <param name="entities">Entities to insert.</param>
        /// <param name="autoSave">Whether to automatically save changes.</param>
        void InsertMany(
            IEnumerable<TEntity> entities, bool autoSave = false);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="autoSave">Whether to automatically save changes.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<TEntity> UpdateAsync(
            TEntity entity, bool autoSave = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="autoSave">Whether to automatically save changes.</param>
        TEntity Update(
            TEntity entity, bool autoSave = false);

        /// <summary>
        /// Updates multiple entities.
        /// </summary>
        /// <param name="entities">Entities to update.</param>
        /// <param name="autoSave">Whether to automatically save changes.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task UpdateManyAsync(
            IEnumerable<TEntity> entities, bool autoSave = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates multiple entities.
        /// </summary>
        /// <param name="entities">Entities to update.</param>
        /// <param name="autoSave">Whether to automatically save changes.</param>
        void UpdateMany(
            IEnumerable<TEntity> entities, bool autoSave = false);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="entity">Entity to delete.</param>
        /// <param name="autoSave">Whether to automatically save changes.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task DeleteAsync(
            TEntity entity, bool autoSave = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="entity">Entity to delete.</param>
        /// <param name="autoSave">Whether to automatically save changes.</param>
        void Delete(
            TEntity entity, bool autoSave = false);

        /// <summary>
        /// Deletes multiple entities.
        /// </summary>
        /// <param name="entities">Entities to delete.</param>
        /// <param name="autoSave">Whether to automatically save changes.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task DeleteManyAsync(
            IEnumerable<TEntity> entities, bool autoSave = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes multiple entities.
        /// </summary>
        /// <param name="entities">Entities to delete.</param>
        /// <param name="autoSave">Whether to automatically save changes.</param>
        void DeleteMany(
            IEnumerable<TEntity> entities, bool autoSave = false);

        /// <summary>
        /// Saves all pending changes.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves all pending changes.
        /// </summary>
        int SaveChanges();
    }
}
