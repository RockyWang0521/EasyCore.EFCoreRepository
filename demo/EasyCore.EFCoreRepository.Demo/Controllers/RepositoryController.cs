using EFCore.Entity;
using EFCore.Repository;
using Microsoft.AspNetCore.Mvc;

namespace EasyCore.EFCoreRepository.Demo.Controllers
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
            return await _repository.GetAsync(e => e.Name == "Test");
        }

        [HttpPost]
        public async Task Post()
        {
            await _repository.InsertAsync(new EFCore.Entity.TestEntity { Name = "Test", Age = 10, Guid = Guid.NewGuid() }, true);
        }

        [HttpPut]
        public async Task Put()
        {
            var entity = await _repository.GetAsync(e => e.Name == "Test");

            entity.Age = 20;

            await _repository.UpdateAsync(entity, true);
        }

        [HttpDelete]
        public async Task Delete()
        {
            var entity = await _repository.GetAsync(e => e.Age == 20);

            await _repository.DeleteAsync(entity, true);
        }
    }
}
