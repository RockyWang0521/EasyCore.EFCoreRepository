using MongoDB.Bson.Serialization.Attributes;

namespace EasyCore.MongoDbRepository.EntityBase
{
    public class EasyCoreEntity<T> : IEntity, IEntitySoftDelete, IEntityTenant, IEntityCreateTime, IEntityUpdateTime
    {
#pragma warning disable CS8618

        /// <summary>
        /// The primary key of the entity.
        /// </summary>
        [BsonId]
        public T Id { get; set; }

        /// <summary>
        /// The tenant id of the entity.
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// The soft delete flag of the entity.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// The create time of the entity.
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// The update time of the entity.
        /// </summary>
        public DateTime? UpdateTime { get; set; }

#pragma warning restore CS8618
    }
}
