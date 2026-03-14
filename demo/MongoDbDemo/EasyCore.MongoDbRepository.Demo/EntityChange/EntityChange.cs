using EasyCore.EntityChange;
using MongoDb.Entity;

namespace EasyCore.MongoDbRepository.Demo.EntityChange
{
    public class EntityChange : IEntityUpdatedChangeHandler<TestEntity>, IEntityDeletedChangeHandler<TestEntity>, IEntityAddedChangeHandler<TestEntity>
    {
        private readonly ILogger<EntityChange> _logger;

        public EntityChange(ILogger<EntityChange> logger) => _logger = logger;

        public Task OnAddedAsync(TestEntity entity)
        {
            _logger.LogInformation("Entity added: Id:{Id}; Name:{Name}; Age:{Age};", entity.Id, entity.Name, entity.Age);
            return Task.CompletedTask;
        }

        public Task OnDeletedAsync(TestEntity entity)
        {
            _logger.LogInformation("Entity deleted: Id:{Id}; Name:{Name}; Age:{Age};", entity.Id, entity.Name, entity.Age);
            return Task.CompletedTask;
        }

        public Task OnUpdatedAsync(TestEntity oldEntity, TestEntity currentEntity)
        {
            _logger.LogInformation(
                "Entity updated: Id:{OldId} --> Id:{NewId}; Name:{OldName} --> Name:{NewName}; Age:{OldAge} --> Age:{NewAge};",
                oldEntity.Id, currentEntity.Id, oldEntity.Name, currentEntity.Name, oldEntity.Age, currentEntity.Age);
            return Task.CompletedTask;
        }
    }
}
