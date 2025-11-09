using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace EasyCore.EFCoreRepository.EntityBase
{
    public class EasyCoreEntity<TKey> : IEntity, IEntityConcurrencyCheck, IEntitySoftDelete, IEntityTenant, IEntityCreateTime, IEntityUpdateTime
    {
#pragma warning disable CS8618

        /// <summary>
        /// The primary key of the entity.
        /// </summary>
        [Key]
        public TKey Id { get; set; }

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
