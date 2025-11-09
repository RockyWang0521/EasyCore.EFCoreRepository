using EasyCore.EFCoreRepository.EntityBase;

namespace EFCore.Entity
{
    public class TestEntity : EasyCoreEntity<Guid>
    {
        public string? Name { get; set; }

        public int Age { get; set; }
    }
}
