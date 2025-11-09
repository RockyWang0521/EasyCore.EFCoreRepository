namespace EasyCore.EFCoreRepository.EntityBase
{
    /// <summary>
    /// Interface for entity with update time
    /// </summary>
    public interface IEntityUpdateTime
    {
        /// <summary>
        /// The last update time of the entity
        /// </summary>
        DateTime? UpdateTime { get; set; }
    }
}
