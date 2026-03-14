using EasyCore.EFCoreRepository.EntityBase;

namespace EFCore.Entity
{
    public class CustomEntity : EasyCoreEntity<Guid>
    {
        public string? CreateId { get; set; }
    }
}
