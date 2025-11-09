namespace EasyCore.MongoDbRepository.EntityBase
{
    /// <summary>
    /// Interface for entity with Soft Delete.
    /// </summary>
    public interface IEntitySoftDelete
    {
        public bool IsDeleted { get; set; }
    }
}
