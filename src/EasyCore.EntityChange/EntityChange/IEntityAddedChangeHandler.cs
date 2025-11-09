namespace EasyCore.EntityChange
{
    /// <summary>
    /// Interface for handling entity added changes.
    /// </summary>
    /// <typeparam name="TEntity">Entity</typeparam>
    public interface IEntityAddedChangeHandler<TEntity> : IEntityChangeHandler
    {
        Task OnAddedAsync(TEntity entity);
    }
}
