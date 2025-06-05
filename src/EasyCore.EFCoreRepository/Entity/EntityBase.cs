using System.ComponentModel.DataAnnotations;

namespace EasyCore.EFCoreRepository.Entity
{
    public class EntityBase : IEntity, IEntityConcurrencyCheck, IEntitySoftDelete, IEntityTenant
    {
        public string? TenantId { get; set; }

        public bool IsDeleted { get; set; }

        [ConcurrencyCheck]
        public string ConcurrencyStamp { get; set; }
    }
}
