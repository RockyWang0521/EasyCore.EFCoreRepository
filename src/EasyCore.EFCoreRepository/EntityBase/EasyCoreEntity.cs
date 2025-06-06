using System.ComponentModel.DataAnnotations;

namespace EasyCore.EFCoreRepository.EntityBase
{
    public class EasyCoreEntity<T> : IEntity, IEntityConcurrencyCheck, IEntitySoftDelete, IEntityTenant
    {
        /// <summary>
        /// The primary key of the entity.
        /// </summary>
        [Key]
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
        /// The concurrency stamp of the entity.
        /// </summary>
        [ConcurrencyCheck]
        public string ConcurrencyStamp { get; set; }
    }
}
