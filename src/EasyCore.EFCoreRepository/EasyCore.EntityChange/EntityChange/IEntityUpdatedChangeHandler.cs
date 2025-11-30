namespace EasyCore.EntityChange
{
    /// <summary>
    /// Interface for handling entity updated changes.
    /// </summary>
    /// <typeparam name="TEntity">Entity</typeparam>
    public interface IEntityUpdatedChangeHandler<TEntity> : IEntityChangeHandler
    {
        Task OnUpdatedAsync(TEntity originalEntity, TEntity currentEntity);
    }
}
