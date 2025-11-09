using EasyCore.MongoDbRepository.EntityBase;

namespace MongoDb.Entity
{
    public class TestEntity : EasyCoreEntity<Guid>
    {
        public string? Name { get; set; }

        public int Age { get; set; }
    }
}
