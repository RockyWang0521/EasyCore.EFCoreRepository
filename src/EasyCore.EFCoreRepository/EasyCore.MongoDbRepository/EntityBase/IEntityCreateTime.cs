namespace EasyCore.MongoDbRepository.EntityBase
{
    /// <summary>
    /// Interface for entity with create time property
    /// </summary>
    public interface IEntityCreateTime
    {
        /// <summary>
        /// The creation time of the entity
        /// </summary>
        DateTime? CreateTime { get; set; }
    }
}
