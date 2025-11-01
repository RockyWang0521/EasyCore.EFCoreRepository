namespace EasyCore.EFCoreEntityChange.EntityChange
{
    /// <summary>
    /// Interface for handling entity deleted changes.
    /// </summary>
    /// <typeparam name="TEntity">Entity</typeparam>
    public interface IEntityDeletedChangeHandler<TEntity> : IEntityChangeHandler
    {
        Task OnDeletedAsync(TEntity entity);
    }
}
