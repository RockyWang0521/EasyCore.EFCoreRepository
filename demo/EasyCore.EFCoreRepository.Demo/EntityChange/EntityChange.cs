using EasyCore.EFCoreEntityChange.EntityChange;
using EFCore.Entity;

namespace EasyCore.EFCoreRepository.Demo.EntityChange
{
    public class EntityChange : IEntityUpdatedChangeHandler<TestEntity, TestEntity>, IEntityDeletedChangeHandler<TestEntity>, IEntityAddedChangeHandler<TestEntity>
    {
        private readonly ILogger<EntityChange> _logger;

        public EntityChange(ILogger<EntityChange> logger) => _logger = logger;

        public async Task OnAddedAsync(TestEntity entity)
        {
            _logger.LogInformation($"Entity added: Id:{entity.Id}; Name:{entity.Name};Age:{entity.Age};");

            await Task.CompletedTask;

            throw new Exception("Simulated exception in OnDeletedAsync");
        }

        public async Task OnDeletedAsync(TestEntity entity)
        {
            _logger.LogInformation($"Entity deleted: Id:{entity.Id}; Name:{entity.Name};Age:{entity.Age};");

            await Task.CompletedTask;
        }

        public Task OnUpdatedAsync(TestEntity oldEntity, TestEntity currentEntity)
        {
            _logger.LogInformation($"Entity updated: Id:{oldEntity.Id} --> Id:{currentEntity.Id}; Name:{oldEntity.Name} --> Name:{currentEntity.Name};Age:{oldEntity.Age} --> Age:{currentEntity.Age};");

            return Task.CompletedTask;
        }
    }
}
