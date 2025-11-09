namespace EasyCore.EntityChange
{
    /// <summary>
    /// Interface for handling entity updated changes.
    /// </summary>
    /// <typeparam name="TOriginalEntity">OriginalEntity</typeparam>
    /// <typeparam name="TCurrentEntity">CurrentEntity</typeparam>
    public interface IEntityUpdatedChangeHandler<TOriginalEntity, TCurrentEntity> : IEntityChangeHandler
    {
        Task OnUpdatedAsync(TOriginalEntity originalEntity, TCurrentEntity currentEntity);
    }
}
