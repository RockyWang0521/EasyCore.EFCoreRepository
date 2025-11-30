namespace EasyCore.EFCoreRepository.EntityChangeEvent
{
    public class EntityChangedEventArgs<TEntity> : EventArgs
    {
        public TEntity Entity { get; }
        public EntityChangeAction Action { get; }

        public EntityChangedEventArgs(TEntity entity, EntityChangeAction action)
        {
            Entity = entity;

            Action = action;
        }
    }
}
