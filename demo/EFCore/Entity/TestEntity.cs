using EasyCore.EFCoreRepository.EntityBase;
using System.ComponentModel.DataAnnotations;

namespace EFCore.Entity
{
    public class TestEntity : EasyCoreEntity
    {
        [Key]
        public Guid Guid { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

    }
}
