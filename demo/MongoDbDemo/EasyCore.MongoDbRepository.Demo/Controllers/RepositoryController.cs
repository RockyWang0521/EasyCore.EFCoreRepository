using EasyCore.MongoDbRepository.DataFilter;
using EasyCore.MongoDbRepository.Demo.DataFilter;
using Microsoft.AspNetCore.Mvc;
using MongoDb.Entity;
using MongoDb.Repository;

namespace EasyCore.MongoDbRepository.Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepositoryController : ControllerBase
    {
        private readonly ITestEntityRepository _repository;

        public RepositoryController(ITestEntityRepository repository) => _repository = repository;

        [HttpGet]
        public async Task<TestEntity> Get()
        {
            return await _repository.GetFirstAsync(e => e.Name == "Test");
        }

        [HttpPost]
        public async Task Post()
        {
            await _repository.InsertAsync(new MongoDb.Entity.TestEntity { Name = "Test", Age = 10, Id = Guid.NewGuid() }, true);
        }

        [HttpPut]
        public async Task Put()
        {
            var entity = await _repository.GetFirstAsync(e => e.Name == "Test");

            if (entity == null) return;

            entity.Age = 20;

            await _repository.UpdateAsync(entity, true);
        }

        [HttpDelete]
        public async Task Delete()
        {
            _repository
                .RemoveFilter(typeof(ITenantFilter))
                .RemoveFilter(typeof(ISoftDeleteFilter))
                .AddFilter(typeof(CustomDataFilter))
                .Delete(e => e.Name == "Test", true);

            _repository.Delete(e => e.Name == "Test", true);

            await Task.CompletedTask;
        }
    }
}
